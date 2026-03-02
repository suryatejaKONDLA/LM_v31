using CITL.SharedKernel.Results;

namespace CITL.Application.Core.FileStorage;

/// <summary>
/// Tenant-aware file storage service — wraps <see cref="IFileStorageProvider"/> with
/// tenant path prefixing, validation, hashing, and ZIP support.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file, prefixing the path with the current tenant identifier.
    /// </summary>
    /// <param name="folder">Relative folder within the tenant bucket (e.g. "invoices/2026").</param>
    /// <param name="fileName">The original filename.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Metadata of the uploaded file.</returns>
    Task<Result<StoredFileMetadata>> UploadAsync(
        string folder,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Downloads a file as a stream.
    /// </summary>
    /// <param name="path">Relative path within the tenant bucket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A download result with stream, content type, and size.</returns>
    Task<Result<FileDownloadResult>> DownloadAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Gets metadata for a file.
    /// </summary>
    /// <param name="path">Relative path within the tenant bucket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>File metadata.</returns>
    Task<Result<StoredFileMetadata>> GetMetadataAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Lists files and sub-folders in the specified folder.
    /// </summary>
    /// <param name="folderPath">Relative folder path within the tenant bucket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Folder contents.</returns>
    Task<Result<FolderContents>> ListAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="path">Relative path within the tenant bucket.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> DeleteAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads all files from one or more folders as a single ZIP archive streamed to the output.
    /// </summary>
    /// <param name="folderPaths">Folder paths to include. Use <c>/</c> for tenant root.</param>
    /// <param name="outputStream">The stream to write the ZIP archive to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> DownloadFolderAsZipAsync(
        IReadOnlyList<string> folderPaths,
        Stream outputStream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Downloads specific files as a single ZIP archive streamed to the output.
    /// </summary>
    /// <param name="paths">List of relative file paths to include.</param>
    /// <param name="outputStream">The stream to write the ZIP archive to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> DownloadFilesAsZipAsync(
        IReadOnlyList<string> paths,
        Stream outputStream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns a recursive tree view of all files and folders under the specified root.
    /// File paths in the tree are relative and can be passed directly to the Download endpoint.
    /// </summary>
    /// <param name="folderPath">Root folder path. Empty for tenant root.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tree structure with summary counts.</returns>
    Task<Result<FileTree>> GetTreeAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a folder within the tenant bucket.
    /// </summary>
    /// <param name="folderPath">Relative folder path to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> CreateFolderAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    /// Generates a pre-signed (or token-based) URL for direct download.
    /// </summary>
    /// <param name="path">Relative path within the tenant bucket.</param>
    /// <param name="expiryMinutes">URL validity in minutes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The signed URL with expiry time.</returns>
    Task<Result<SignedUrlResult>> GetSignedUrlAsync(
        string path,
        int expiryMinutes,
        CancellationToken cancellationToken);
}
