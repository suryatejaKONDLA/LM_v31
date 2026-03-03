using CITL.Application.Core.FileStorage;
using CITL.SharedKernel.Helpers;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.FileStorage;

/// <summary>
/// Multi-tenant file storage — upload, download, list, delete, ZIP, and signed URLs.
/// Files are automatically isolated per tenant via path prefixing.
/// </summary>
/// <remarks>
/// Parameter types used across all endpoints:
/// <list type="number">
///   <item><c>folder</c> — folder path only, no filename: <c>invoices/2026</c> or empty string for root</item>
///   <item><c>path</c> — full file path including filename: <c>invoices/2026/report.pdf</c></item>
///   <item>The <c>path</c> returned by Upload / List / Tree is what you pass to Download, Metadata, Delete, and SignedUrl</item>
/// </list>
/// </remarks>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Common)]
[RequestSizeLimit(1_073_741_824)] // 1 GB
public sealed class FileStorageController(IFileStorageService fileStorageService) : CitlControllerBase
{
    /// <summary>Uploads a file to the specified folder within the tenant bucket.</summary>
    /// <remarks>
    /// Filename resolution, placeholders, allowed extensions, and usage examples:
    /// <list type="number">
    ///   <item>Custom <c>fileName</c> provided → use it (with placeholder expansion)</item>
    ///   <item><c>autoGenerateName=true</c> and no <c>fileName</c> → 32-char GUID + original extension</item>
    ///   <item>Neither → original uploaded filename is used</item>
    ///   <item>If <c>fileName</c> has no extension, the original file extension is appended automatically</item>
    ///   <item><c>{id}</c> placeholder → 32-char GUID, no hyphens, e.g. <c>a1b2c3d4e5f67890a1b2c3d4e5f67890</c></item>
    ///   <item><c>{dt}</c> placeholder → UTC timestamp <c>yyyyMMddHHmmss</c>, e.g. <c>20260301143025</c></item>
    ///   <item>Allowed extensions configured in <c>FileStorage:AllowedExtensions</c> (appsettings.json)</item>
    ///   <item>Blocked by default: <c>exe</c>, <c>ps1</c>, <c>sh</c>, <c>bat</c>, <c>dll</c> — uploading returns 400</item>
    ///   <item>Root upload: <c>POST /FileStorage/Upload?folder=</c></item>
    ///   <item>Folder upload: <c>POST /FileStorage/Upload?folder=invoices/2026</c></item>
    ///   <item>Custom name: <c>POST /FileStorage/Upload?folder=logos&amp;fileName=company-logo.png</c></item>
    ///   <item>Name without ext: <c>POST /FileStorage/Upload?fileName=my-report</c> → auto-appends <c>.pdf</c></item>
    ///   <item>Auto-generate name: <c>POST /FileStorage/Upload?autoGenerateName=true</c></item>
    ///   <item>Placeholder {id}: <c>POST /FileStorage/Upload?fileName=Invoice_{id}</c> → <c>Invoice_a1b2c3d4.pdf</c></item>
    ///   <item>Placeholder {dt}: <c>POST /FileStorage/Upload?fileName=Report_{dt}</c> → <c>Report_20260301143025.pdf</c></item>
    ///   <item>Both placeholders: <c>POST /FileStorage/Upload?fileName=Backup_{id}_{dt}</c></item>
    /// </list>
    /// </remarks>
    /// <param name="file">The file to upload (multipart/form-data).</param>
    /// <param name="folder">Target folder path, e.g. <c>invoices/2026</c>. Leave empty for root.</param>
    /// <param name="fileName">Custom save name. Supports <c>{id}</c> and <c>{dt}</c> placeholders. Extension auto-appended if omitted.</param>
    /// <param name="autoGenerateName">Generate a unique GUID filename when no <c>fileName</c> is given. Preserves original extension.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Metadata of the uploaded file — use the returned <c>path</c> for Download / Delete / Metadata calls.</returns>
    /// <response code="200">File uploaded successfully.</response>
    /// <response code="400">Validation error — empty file, blocked extension, invalid path, or size limit exceeded.</response>
    [HttpPost("Upload")]
    [ProducesResponseType(typeof(ApiResponse<StoredFileMetadata>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAsync(
        IFormFile file,
        [FromQuery] string folder = "",
        [FromQuery] string? fileName = null,
        [FromQuery] bool autoGenerateName = false,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse.Error("No file provided or file is empty."));
        }

        // Resolve filename: custom > autoGenerate > original
        var resolvedFileName = FileNameHelper.ResolveFileName(file.FileName, fileName, autoGenerateName);

        await using var stream = file.OpenReadStream();

        var result = await fileStorageService.UploadAsync(
            folder,
            resolvedFileName,
            stream,
            file.ContentType ?? "application/octet-stream",
            cancellationToken);

        return FromResult(result, "File uploaded successfully.");
    }

    /// <summary>Downloads a file, streamed directly with <c>Content-Length</c> for progress tracking.</summary>
    /// <remarks>
    /// How to download a file:
    /// <list type="number">
    ///   <item>Pass the <c>path</c> value returned by Upload, List, or Tree endpoints</item>
    ///   <item>Response includes <c>Content-Length</c> so clients can show a progress bar</item>
    ///   <item>Supports HTTP range requests (<c>Accept-Ranges: bytes</c>) for resume / partial download</item>
    ///   <item>Example: <c>GET /FileStorage/Download?path=invoices/2026/report.pdf</c></item>
    ///   <item>Example: <c>GET /FileStorage/Download?path=logo.png</c></item>
    /// </list>
    /// </remarks>
    /// <param name="path">Full relative file path returned by Upload / List / Tree, e.g. <c>invoices/2026/report.pdf</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw file stream with correct <c>Content-Type</c> and <c>Content-Disposition</c> headers.</returns>
    /// <response code="200">File stream.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("Download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAsync(
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.DownloadAsync(path, cancellationToken);

        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        var download = result.Value;

        Response.Headers.ContentLength = download.SizeInBytes;

        return File(download.Content, download.ContentType, download.FileName, enableRangeProcessing: true);
    }

    /// <summary>Gets metadata for a file — size, hash, content type, and timestamps.</summary>
    /// <remarks>
    /// Returned metadata fields and usage examples:
    /// <list type="number">
    ///   <item><c>FileName</c> — name of the file</item>
    ///   <item><c>FilePath</c> — relative path (use this for Download / Delete)</item>
    ///   <item><c>ContentType</c> — MIME type, e.g. <c>application/pdf</c></item>
    ///   <item><c>SizeInBytes</c> — file size in bytes</item>
    ///   <item><c>Hash</c> — MD5 or ETag for integrity checking</item>
    ///   <item><c>CreatedAtUtc</c>, <c>LastModifiedAtUtc</c> — timestamps</item>
    ///   <item>Example: <c>GET /FileStorage/Metadata?path=invoices/2026/report.pdf</c></item>
    ///   <item>Example: <c>GET /FileStorage/Metadata?path=logo.png</c></item>
    /// </list>
    /// </remarks>
    /// <param name="path">Full relative file path, e.g. <c>invoices/2026/report.pdf</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>File metadata object.</returns>
    /// <response code="200">File metadata.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("Metadata")]
    [ProducesResponseType(typeof(ApiResponse<StoredFileMetadata>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetadataAsync(
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.GetMetadataAsync(path, cancellationToken);

        return FromResult(result);
    }

    /// <summary>Lists files and sub-folders in the specified folder (single level, non-recursive).</summary>
    /// <remarks>
    /// Returned fields and usage examples:
    /// <list type="number">
    ///   <item><c>Files</c> — list of file metadata objects in this folder</item>
    ///   <item><c>SubFolders</c> — list of immediate child folder names</item>
    ///   <item>Each file entry includes a <c>FilePath</c> you can pass to Download / Metadata / Delete</item>
    ///   <item>Root: <c>GET /FileStorage/List?folder=</c></item>
    ///   <item>Sub-folder: <c>GET /FileStorage/List?folder=invoices/2026</c></item>
    ///   <item>Nested path: <c>GET /FileStorage/List?folder=documents/contracts/2025</c></item>
    /// </list>
    /// </remarks>
    /// <param name="folder">Folder path to list, e.g. <c>invoices/2026</c>. Leave empty for root.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Folder contents with files and immediate sub-folders.</returns>
    /// <response code="200">Folder listing.</response>
    [HttpGet("List")]
    [ProducesResponseType(typeof(ApiResponse<FolderContents>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] string folder = "",
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.ListAsync(folder, cancellationToken);

        return FromResult(result);
    }

    /// <summary>Returns a full recursive tree of all files and sub-folders starting from the given folder.</summary>
    /// <remarks>
    /// Returned fields and usage examples:
    /// <list type="number">
    ///   <item><c>Root</c> — root node containing nested <c>Children</c> (folders) and <c>Files</c></item>
    ///   <item><c>TotalFiles</c>, <c>TotalFolders</c> — summary counts across the entire tree</item>
    ///   <item>Each file node has a <c>Path</c> field — pass it directly to Download, Metadata, Delete, or SignedUrl</item>
    ///   <item>Full tree: <c>GET /FileStorage/Tree?folder=</c></item>
    ///   <item>Sub-tree: <c>GET /FileStorage/Tree?folder=invoices</c></item>
    ///   <item>Deep path: <c>GET /FileStorage/Tree?folder=documents/archive/2025</c></item>
    /// </list>
    /// </remarks>
    /// <param name="folder">Folder to start from, e.g. <c>invoices</c>. Leave empty for the tenant root.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Recursive tree with file nodes and summary counts.</returns>
    /// <response code="200">File tree.</response>
    [HttpGet("Tree")]
    [ProducesResponseType(typeof(ApiResponse<FileTree>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeAsync(
        [FromQuery] string folder = "",
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.GetTreeAsync(folder, cancellationToken);

        return FromResult(result);
    }

    /// <summary>Permanently deletes a file from the tenant bucket.</summary>
    /// <remarks>
    /// How to delete a file:
    /// <list type="number">
    ///   <item>Pass the <c>path</c> returned by Upload, List, or Tree</item>
    ///   <item>This action is irreversible — there is no recycle bin or soft delete</item>
    ///   <item>Example: <c>DELETE /FileStorage?path=invoices/2026/report.pdf</c></item>
    ///   <item>Example: <c>DELETE /FileStorage?path=logo.png</c></item>
    /// </list>
    /// </remarks>
    /// <param name="path">Full relative file path to delete, e.g. <c>invoices/2026/report.pdf</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">File deleted.</response>
    /// <response code="404">File not found.</response>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.DeleteAsync(path, cancellationToken);

        return FromResult(result, "File deleted successfully.");
    }

    /// <summary>Downloads all files from one or more folders as a single ZIP archive.</summary>
    /// <remarks>
    /// Folder ZIP behaviour, notes, and examples:
    /// <list type="number">
    ///   <item>Recursively includes every file under each specified folder</item>
    ///   <item>Use <c>/</c> to include all files from the tenant root</item>
    ///   <item>Multiple folders are merged into a single flat ZIP</item>
    ///   <item>Response is streamed — no temporary file is buffered on the server</item>
    ///   <item>Response header: <c>Content-Disposition: attachment; filename="{guid}.zip"</c></item>
    ///   <item>Root folder: <c>{ "FolderPaths": ["/"] }</c></item>
    ///   <item>Single folder: <c>{ "FolderPaths": ["invoices/2026"] }</c></item>
    ///   <item>Multiple folders: <c>{ "FolderPaths": ["invoices/2026", "reports/q1"] }</c></item>
    /// </list>
    /// </remarks>
    /// <param name="request">Request body with one or more folder paths to ZIP.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Streamed ZIP archive.</returns>
    /// <response code="200">ZIP archive stream.</response>
    /// <response code="400">No folder paths were provided.</response>
    /// <response code="404">No files found in the specified folders.</response>
    [HttpPost("Zip/Folder")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFolderAsZipAsync(
        [FromBody] ZipFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        // ZipArchive.Dispose writes the central directory synchronously — allow it for this request
        EnableSynchronousIO();

        Response.ContentType = "application/zip";
        var zipFileName = $"{Guid.NewGuid():N}.zip";
        Response.Headers.ContentDisposition = $"attachment; filename=\"{zipFileName}\"";

        var result = await fileStorageService.DownloadFolderAsZipAsync(
            request.FolderPaths,
            Response.Body,
            cancellationToken);

        if (!result.IsSuccess && !Response.HasStarted)
        {
            Response.Clear();
            return FromResult(result);
        }

        return new EmptyResult();
    }

    /// <summary>Downloads specific files as a single ZIP archive.</summary>
    /// <remarks>
    /// File-based ZIP behaviour, notes, and examples:
    /// <list type="number">
    ///   <item>Include only the files you specify — no folder traversal</item>
    ///   <item>Use file paths exactly as returned by Upload, List, or Tree endpoints</item>
    ///   <item>Non-existent paths are silently skipped</item>
    ///   <item>Response is streamed — no temporary file is buffered on the server</item>
    ///   <item>Response header: <c>Content-Disposition: attachment; filename="{guid}.zip"</c></item>
    ///   <item>Example body: <c>{ "FilePaths": ["invoices/jan.pdf", "invoices/feb.pdf"] }</c></item>
    /// </list>
    /// </remarks>
    /// <param name="request">Request body with the list of file paths to ZIP.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Streamed ZIP archive.</returns>
    /// <response code="200">ZIP archive stream.</response>
    /// <response code="400">No file paths were provided.</response>
    [HttpPost("Zip/Files")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadFilesAsZipAsync(
        [FromBody] ZipFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        // ZipArchive.Dispose writes the central directory synchronously — allow it for this request
        EnableSynchronousIO();

        Response.ContentType = "application/zip";
        var zipFileName = $"{Guid.NewGuid():N}.zip";
        Response.Headers.ContentDisposition = $"attachment; filename=\"{zipFileName}\"";

        var result = await fileStorageService.DownloadFilesAsZipAsync(
            request.FilePaths,
            Response.Body,
            cancellationToken);

        if (!result.IsSuccess && !Response.HasStarted)
        {
            Response.Clear();
            return FromResult(result);
        }

        return new EmptyResult();
    }

    /// <summary>Creates an empty folder within the tenant bucket.</summary>
    /// <remarks>
    /// Provider-specific behaviour and usage examples:
    /// <list type="number">
    ///   <item>For Local provider: creates a physical directory on disk</item>
    ///   <item>For R2 provider: creates a zero-byte placeholder object with a trailing slash</item>
    ///   <item>Folders are created recursively — intermediate paths are created automatically</item>
    ///   <item>Example: <c>POST /FileStorage/Folders?path=invoices/2026</c></item>
    ///   <item>Example: <c>POST /FileStorage/Folders?path=documents/archive/2025/Q1</c></item>
    /// </list>
    /// </remarks>
    /// <param name="path">Folder path to create, e.g. <c>invoices/2026</c>. Must not be a filename.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Folder created.</response>
    /// <response code="400">Path is empty or contains invalid characters.</response>
    [HttpPost("Folders")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFolderAsync(
        [FromQuery] string path,
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.CreateFolderAsync(path, cancellationToken);

        return FromResult(result, "Folder created successfully.");
    }

    /// <summary>Generates a time-limited pre-signed URL for direct file download.</summary>
    /// <remarks>
    /// Provider-specific behaviour, expiry settings, and usage examples:
    /// <list type="number">
    ///   <item><b>R2 (Cloudflare)</b> — returns a real pre-signed S3 URL; the client downloads directly from R2</item>
    ///   <item><b>Local</b> — returns an API-backed URL (the token is validated on the next request)</item>
    ///   <item>Default expiry is 15 minutes (configurable via <c>FileStorage:SignedUrlExpiryMinutes</c>)</item>
    ///   <item>URL includes an expiry timestamp — accessing after expiry returns 403/404</item>
    ///   <item>No authentication header needed when using the signed URL directly</item>
    ///   <item><c>GET /FileStorage/SignedUrl?path=invoices/2026/report.pdf</c> → 15-minute URL</item>
    ///   <item><c>GET /FileStorage/SignedUrl?path=logo.png&amp;expiryMinutes=60</c> → 1-hour URL</item>
    ///   <item><c>GET /FileStorage/SignedUrl?path=contracts/NDA.pdf&amp;expiryMinutes=1440</c> → 24-hour URL</item>
    /// </list>
    /// </remarks>
    /// <param name="path">Full relative file path, e.g. <c>invoices/2026/report.pdf</c>.</param>
    /// <param name="expiryMinutes">How long the URL stays valid. Defaults to <c>15</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Signed URL string with expiry metadata.</returns>
    /// <response code="200">Signed URL generated.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("SignedUrl")]
    [ProducesResponseType(typeof(ApiResponse<SignedUrlResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSignedUrlAsync(
        [FromQuery] string path,
        [FromQuery] int expiryMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await fileStorageService.GetSignedUrlAsync(path, expiryMinutes, cancellationToken);

        return FromResult(result);
    }

    // ─── Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Enables synchronous IO for the current request so that ZipArchive.Dispose
    /// can write the central directory synchronously to the response stream.
    /// </summary>
    private void EnableSynchronousIO()
    {
        if (HttpContext.Features.Get<IHttpBodyControlFeature>() is { } feature)
        {
            feature.AllowSynchronousIO = true;
        }
    }
}
