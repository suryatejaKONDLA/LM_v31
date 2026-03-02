using System.IO.Compression;
using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Helpers;
using CITL.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.FileStorage;

/// <summary>
/// Tenant-aware file storage service — prefixes all paths with the current tenant ID,
/// validates inputs, and delegates to <see cref="IFileStorageProvider"/>.
/// </summary>
public sealed partial class FileStorageService(
    IFileStorageProvider provider,
    ITenantContext tenantContext,
    FileStorageUploadSettings uploadSettings,
    ILogger<FileStorageService> logger) : IFileStorageService
{
    private const long MaxFileSizeBytes = 1_073_741_824; // 1 GB

    /// <inheritdoc />
    public async Task<Result<StoredFileMetadata>> UploadAsync(
        string folder,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.FileNameRequired", "File name is required."));
        }

        if (content is { CanSeek: true, Length: > MaxFileSizeBytes })
        {
            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.FileTooLarge", "File exceeds the 1 GB size limit."));
        }

        // Validate file extension against allowed list
        var extension = Path.GetExtension(fileName);

        if (uploadSettings.AllowedExtensions.Count > 0
            && !uploadSettings.AllowedExtensions.Contains(extension))
        {
            LogExtensionRejected(logger, tenantContext.TenantId, extension);

            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.ExtensionNotAllowed",
                    $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", uploadSettings.AllowedExtensions)}."));
        }

        var sanitizedFolder = PathSanitizer.SanitizePath(folder);
        var sanitizedFileName = PathSanitizer.SanitizeFileName(fileName);

        if (sanitizedFolder is null || sanitizedFileName is null)
        {
            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var fullPath = string.IsNullOrEmpty(sanitizedFolder)
            ? sanitizedFileName
            : $"{sanitizedFolder}/{sanitizedFileName}";

        var tenantPath = BuildTenantPath(fullPath);

        LogUploadStarted(logger, tenantContext.TenantId, fullPath, contentType);

        var metadata = await provider.UploadAsync(tenantPath, content, contentType, cancellationToken)
            .ConfigureAwait(false);

        // Return metadata with tenant-relative path (strip tenant prefix)
        var result = new StoredFileMetadata
        {
            FileName = metadata.FileName,
            FilePath = fullPath,
            Extension = metadata.Extension,
            ContentType = metadata.ContentType,
            SizeInBytes = metadata.SizeInBytes,
            Hash = metadata.Hash,
            CreatedAtUtc = metadata.CreatedAtUtc,
            LastModifiedAtUtc = metadata.LastModifiedAtUtc
        };

        LogUploadCompleted(logger, tenantContext.TenantId, fullPath, result.SizeInBytes);

        return result;
    }

    /// <inheritdoc />
    public async Task<Result<FileDownloadResult>> DownloadAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<FileDownloadResult>(
                Error.Validation("FileStorage.PathRequired", "File path is required."));
        }

        var sanitized = PathSanitizer.SanitizePath(path);

        if (sanitized is null)
        {
            return Result.Failure<FileDownloadResult>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);

        var exists = await provider.ExistsAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure<FileDownloadResult>(
                Error.NotFound("FileStorage.FileNotFound", $"File not found: {sanitized}"));
        }

        LogDownloadStarted(logger, tenantContext.TenantId, sanitized);

        var result = await provider.DownloadAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task<Result<StoredFileMetadata>> GetMetadataAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.PathRequired", "File path is required."));
        }

        var sanitized = PathSanitizer.SanitizePath(path);

        if (sanitized is null)
        {
            return Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);
        var metadata = await provider.GetMetadataAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        if (metadata is null)
        {
            return Result.Failure<StoredFileMetadata>(
                Error.NotFound("FileStorage.FileNotFound", $"File not found: {sanitized}"));
        }

        // Return with tenant-relative path
        return new StoredFileMetadata
        {
            FileName = metadata.FileName,
            FilePath = sanitized,
            Extension = metadata.Extension,
            ContentType = metadata.ContentType,
            SizeInBytes = metadata.SizeInBytes,
            Hash = metadata.Hash,
            CreatedAtUtc = metadata.CreatedAtUtc,
            LastModifiedAtUtc = metadata.LastModifiedAtUtc
        };
    }

    /// <inheritdoc />
    public async Task<Result<FolderContents>> ListAsync(
        string folderPath,
        CancellationToken cancellationToken)
    {
        var sanitized = PathSanitizer.SanitizePath(folderPath ?? string.Empty);

        if (sanitized is null)
        {
            return Result.Failure<FolderContents>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);
        var contents = await provider.ListAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        // Strip tenant prefix from returned paths
        var tenantPrefix = $"{tenantContext.TenantId}/";
        var items = contents.Items.Select(item => new FolderItem
        {
            Name = item.Name,
            Path = item.Path.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase)
                ? item.Path[tenantPrefix.Length..]
                : item.Path,
            IsFolder = item.IsFolder,
            SizeInBytes = item.SizeInBytes,
            ContentType = item.ContentType,
            LastModifiedAtUtc = item.LastModifiedAtUtc
        }).ToList();

        return new FolderContents
        {
            FolderPath = sanitized,
            Items = items,
            TotalCount = items.Count
        };
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure(
                Error.Validation("FileStorage.PathRequired", "File path is required."));
        }

        var sanitized = PathSanitizer.SanitizePath(path);

        if (sanitized is null)
        {
            return Result.Failure(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);
        var exists = await provider.ExistsAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure(
                Error.NotFound("FileStorage.FileNotFound", $"File not found: {sanitized}"));
        }

        await provider.DeleteAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        LogFileDeleted(logger, tenantContext.TenantId, sanitized);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DownloadFolderAsZipAsync(
        IReadOnlyList<string> folderPaths,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var filtered = folderPaths
            .Select(p => p == "/" ? string.Empty : p)
            .Where(p => p is not null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (filtered.Count == 0)
        {
            return Result.Failure(
                Error.Validation("FileStorage.ZipRequestEmpty",
                    "Specify at least one folder path for ZIP download."));
        }

        var filePaths = new List<string>();

        foreach (var folder in filtered)
        {
            var sanitizedFolder = PathSanitizer.SanitizePath(folder);

            if (sanitizedFolder is null)
            {
                return Result.Failure(
                    Error.Validation("FileStorage.InvalidPath", $"Folder path contains invalid characters: {folder}"));
            }

            var tenantFolder = BuildTenantPath(sanitizedFolder);
            var allFiles = await provider.ListAllRecursiveAsync(tenantFolder, cancellationToken).ConfigureAwait(false);
            filePaths.AddRange(allFiles.Select(i => i.Path));
        }

        if (filePaths.Count == 0)
        {
            return Result.Failure(
                Error.NotFound("FileStorage.NoFilesFound", "No files found to include in the ZIP archive."));
        }

        // Deduplicate in case overlapping folders were requested (e.g. "invoices" + "invoices/2026")
        var uniqueFilePaths = filePaths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return await WriteZipAsync(uniqueFilePaths, outputStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result> DownloadFilesAsZipAsync(
        IReadOnlyList<string> paths,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var filtered = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

        if (filtered.Count == 0)
        {
            return Result.Failure(
                Error.Validation("FileStorage.ZipRequestEmpty",
                    "Specify at least one file path for ZIP download."));
        }

        var filePaths = new List<string>(filtered.Count);

        foreach (var path in filtered)
        {
            var sanitized = PathSanitizer.SanitizePath(path);

            if (sanitized is null)
            {
                return Result.Failure(
                    Error.Validation("FileStorage.InvalidPath", $"Invalid file path: {path}"));
            }

            filePaths.Add(BuildTenantPath(sanitized));
        }

        return await WriteZipAsync(filePaths, outputStream, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result> WriteZipAsync(
        List<string> filePaths,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        LogZipStarted(logger, tenantContext.TenantId, filePaths.Count);

        using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true);

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await provider.ExistsAsync(filePath, cancellationToken).ConfigureAwait(false);

            if (!exists)
            {
                continue;
            }

            using var download = await provider.DownloadAsync(filePath, cancellationToken).ConfigureAwait(false);

            // Use the relative path (strip tenant prefix) as entry name to preserve structure
            // and avoid collisions between same-named files in different folders
            var tenantPrefix = $"{tenantContext.TenantId}/";
            var entryName = filePath.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase)
                ? filePath[tenantPrefix.Length..]
                : filePath;

            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

            var entryStream = entry.Open();
            await using (entryStream.ConfigureAwait(false))
            {
                await download.Content.CopyToAsync(entryStream, cancellationToken).ConfigureAwait(false);
            }
        }

        LogZipCompleted(logger, tenantContext.TenantId, filePaths.Count);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<FileTree>> GetTreeAsync(
        string folderPath,
        CancellationToken cancellationToken)
    {
        var sanitized = PathSanitizer.SanitizePath(folderPath ?? string.Empty);

        if (sanitized is null)
        {
            return Result.Failure<FileTree>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);
        var allFiles = await provider.ListAllRecursiveAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        // Strip tenant prefix from all paths
        var tenantPrefix = $"{tenantContext.TenantId}/";
        var rootPrefix = string.IsNullOrEmpty(sanitized) ? string.Empty : $"{sanitized}/";

        var totalFiles = 0;
        var totalSizeInBytes = 0L;
        var folderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build lookup: folder-path → list of nodes
        var rootNodes = new List<FileTreeNode>();
        var folderNodes = new Dictionary<string, FileTreeNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in allFiles)
        {
            var relativePath = file.Path.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase)
                ? file.Path[tenantPrefix.Length..]
                : file.Path;

            // Path within the requested folder (strip the root prefix for tree building)
            var treePath = !string.IsNullOrEmpty(rootPrefix) && relativePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
                ? relativePath[rootPrefix.Length..]
                : relativePath;

            var segments = treePath.Split('/');

            // Ensure all parent folder nodes exist
            var currentChildren = rootNodes;
            var currentPath = string.Empty;

            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";

                if (!folderNodes.TryGetValue(currentPath, out var existingFolder))
                {
                    existingFolder = new()
                    {
                        Name = segment,
                        Path = string.Empty, // Folders don't have a download path
                        IsFolder = true,
                        Children = []
                    };

                    folderNodes[currentPath] = existingFolder;
                    currentChildren.Add(existingFolder);
                    folderSet.Add(currentPath);
                }

                currentChildren = existingFolder.Children;
            }

            // Add the file node
            var fileNode = new FileTreeNode
            {
                Name = segments[^1],
                Path = relativePath, // Full tenant-relative path for download
                IsFolder = false,
                SizeInBytes = file.SizeInBytes,
                ContentType = file.ContentType,
                LastModifiedAtUtc = file.LastModifiedAtUtc
            };

            currentChildren.Add(fileNode);
            totalFiles++;
            totalSizeInBytes += file.SizeInBytes;
        }

        return new FileTree
        {
            RootPath = sanitized,
            Nodes = rootNodes,
            TotalFiles = totalFiles,
            TotalFolders = folderSet.Count,
            TotalSizeInBytes = totalSizeInBytes
        };
    }

    /// <inheritdoc />
    public async Task<Result> CreateFolderAsync(string folderPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return Result.Failure(
                Error.Validation("FileStorage.FolderPathRequired", "Folder path is required."));
        }

        var sanitized = PathSanitizer.SanitizePath(folderPath);

        if (sanitized is null)
        {
            return Result.Failure(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);

        await provider.CreateFolderAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        LogFolderCreated(logger, tenantContext.TenantId, sanitized);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<SignedUrlResult>> GetSignedUrlAsync(
        string path,
        int expiryMinutes,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<SignedUrlResult>(
                Error.Validation("FileStorage.PathRequired", "File path is required."));
        }

        if (expiryMinutes <= 0)
        {
            return Result.Failure<SignedUrlResult>(
                Error.Validation("FileStorage.InvalidExpiry", "Expiry minutes must be greater than zero."));
        }

        var sanitized = PathSanitizer.SanitizePath(path);

        if (sanitized is null)
        {
            return Result.Failure<SignedUrlResult>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters."));
        }

        var tenantPath = BuildTenantPath(sanitized);
        var exists = await provider.ExistsAsync(tenantPath, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure<SignedUrlResult>(
                Error.NotFound("FileStorage.FileNotFound", $"File not found: {sanitized}"));
        }

        var result = await provider.GetSignedUrlAsync(tenantPath, expiryMinutes, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    // ─── Helpers ────────────────────────────────────────────────────────

    private string BuildTenantPath(string relativePath)
    {
        var tenantId = tenantContext.TenantId;

        return string.IsNullOrEmpty(relativePath)
            ? tenantId
            : $"{tenantId}/{relativePath}";
    }

    // ─── Source-generated log messages ──────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "File upload started — Tenant: {TenantId}, Path: {FilePath}, ContentType: {ContentType}")]
    private static partial void LogUploadStarted(ILogger logger, string tenantId, string filePath, string contentType);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "File upload completed — Tenant: {TenantId}, Path: {FilePath}, Size: {SizeInBytes} bytes")]
    private static partial void LogUploadCompleted(ILogger logger, string tenantId, string filePath, long sizeInBytes);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "File download started — Tenant: {TenantId}, Path: {FilePath}")]
    private static partial void LogDownloadStarted(ILogger logger, string tenantId, string filePath);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "File deleted — Tenant: {TenantId}, Path: {FilePath}")]
    private static partial void LogFileDeleted(ILogger logger, string tenantId, string filePath);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "ZIP download started — Tenant: {TenantId}, FileCount: {FileCount}")]
    private static partial void LogZipStarted(ILogger logger, string tenantId, int fileCount);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "ZIP download completed — Tenant: {TenantId}, FileCount: {FileCount}")]
    private static partial void LogZipCompleted(ILogger logger, string tenantId, int fileCount);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Folder created — Tenant: {TenantId}, Path: {FolderPath}")]
    private static partial void LogFolderCreated(ILogger logger, string tenantId, string folderPath);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "File upload rejected — Tenant: {TenantId}, Extension: {Extension} is not in the allowed list")]
    private static partial void LogExtensionRejected(ILogger logger, string tenantId, string extension);
}
