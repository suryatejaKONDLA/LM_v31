using System.Security.Cryptography;
using CITL.Application.Core.FileStorage;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.Core.FileStorage;

/// <summary>
/// Local disk file storage provider — stores files on the local file system or shared drive.
/// Tenant isolation is handled by the service layer (path prefixing).
/// </summary>
internal sealed partial class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageProvider> _logger;
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    /// Platform-aware path comparison: case-insensitive on Windows/macOS, case-sensitive on Linux.
    /// </summary>
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsLinux() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    /// <summary>Initializes the local provider with configuration and logger.</summary>
    public LocalFileStorageProvider(
        IOptions<FileStorageSettings> options,
        ILogger<LocalFileStorageProvider> logger)
    {
        // Ensure base path ends with separator so StartsWith can't match partial directory names
        // e.g. "/storage" must not match "/storage-other/file.txt"
        var fullPath = Path.GetFullPath(options.Value.LocalBasePath);
        _basePath = fullPath.EndsWith(Path.DirectorySeparatorChar)
            ? fullPath
            : fullPath + Path.DirectorySeparatorChar;
        _logger = logger;

        Directory.CreateDirectory(_basePath);
    }

    /// <inheritdoc />
    public async Task<StoredFileMetadata> UploadAsync(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Stream to file while computing SHA256 hash
        string hash;
        long sizeInBytes;

        using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous);

        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        var totalBytes = 0L;
        int bytesRead;

        while ((bytesRead = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken)
                .ConfigureAwait(false);

            sha256.AppendData(buffer, 0, bytesRead);
            totalBytes += bytesRead;
        }

        sizeInBytes = totalBytes;
        hash = Convert.ToHexString(sha256.GetCurrentHash());

        var fileInfo = new FileInfo(fullPath);

        return new()
        {
            FileName = fileInfo.Name,
            FilePath = path,
            Extension = fileInfo.Extension,
            ContentType = contentType,
            SizeInBytes = sizeInBytes,
            Hash = hash,
            CreatedAtUtc = GetCreationTimeUtc(fileInfo),
            LastModifiedAtUtc = fileInfo.LastWriteTimeUtc
        };
    }

    /// <inheritdoc />
    public Task<FileDownloadResult> DownloadAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {path}", path);
        }

        var fileInfo = new FileInfo(fullPath);
        var contentType = ResolveContentType(fileInfo.Name);

        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var result = new FileDownloadResult
        {
            Content = stream,
            ContentType = contentType,
            FileName = fileInfo.Name,
            SizeInBytes = fileInfo.Length
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            LogFileDeleted(_logger, path);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(path);

        return Task.FromResult(File.Exists(fullPath));
    }

    /// <inheritdoc />
    public Task<StoredFileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<StoredFileMetadata?>(null);
        }

        var fileInfo = new FileInfo(fullPath);

        var metadata = new StoredFileMetadata
        {
            FileName = fileInfo.Name,
            FilePath = path,
            Extension = fileInfo.Extension,
            ContentType = ResolveContentType(fileInfo.Name),
            SizeInBytes = fileInfo.Length,
            Hash = string.Empty, // Computing hash would require reading entire file
            CreatedAtUtc = GetCreationTimeUtc(fileInfo),
            LastModifiedAtUtc = fileInfo.LastWriteTimeUtc
        };

        return Task.FromResult<StoredFileMetadata?>(metadata);
    }

    /// <inheritdoc />
    public Task<FolderContents> ListAsync(string folderPath, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(folderPath);

        if (!Directory.Exists(fullPath))
        {
            return Task.FromResult(new FolderContents
            {
                FolderPath = folderPath,
                Items = [],
                TotalCount = 0
            });
        }

        var items = new List<FolderItem>();

        // Sub-directories
        foreach (var dir in Directory.GetDirectories(fullPath))
        {
            var dirInfo = new DirectoryInfo(dir);
            var relativePath = Path.GetRelativePath(_basePath, dir).Replace('\\', '/');

            items.Add(new()
            {
                Name = dirInfo.Name,
                Path = relativePath,
                IsFolder = true,
                SizeInBytes = 0,
                ContentType = string.Empty,
                LastModifiedAtUtc = dirInfo.LastWriteTimeUtc
            });
        }

        // Files
        foreach (var file in Directory.GetFiles(fullPath))
        {
            var fileInfo = new FileInfo(file);
            var relativePath = Path.GetRelativePath(_basePath, file).Replace('\\', '/');

            items.Add(new()
            {
                Name = fileInfo.Name,
                Path = relativePath,
                IsFolder = false,
                SizeInBytes = fileInfo.Length,
                ContentType = ResolveContentType(fileInfo.Name),
                LastModifiedAtUtc = fileInfo.LastWriteTimeUtc
            });
        }

        var contents = new FolderContents
        {
            FolderPath = folderPath,
            Items = items,
            TotalCount = items.Count
        };

        return Task.FromResult(contents);
    }

    /// <inheritdoc />
    public Task<SignedUrlResult> GetSignedUrlAsync(
        string path,
        int expiryMinutes,
        CancellationToken cancellationToken)
    {
        // Local provider returns an API-backed download URL
        // The controller handles the actual file serving
        var result = new SignedUrlResult
        {
            Url = $"/FileStorage/Download?path={Uri.EscapeDataString(path)}",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<FolderItem>> ListAllRecursiveAsync(
        string folderPath,
        CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(folderPath);

        if (!Directory.Exists(fullPath))
        {
            return Task.FromResult<IReadOnlyList<FolderItem>>([]);
        }

        var items = new List<FolderItem>();

        foreach (var file in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(file);
            var relativePath = Path.GetRelativePath(_basePath, file).Replace('\\', '/');

            items.Add(new()
            {
                Name = fileInfo.Name,
                Path = relativePath,
                IsFolder = false,
                SizeInBytes = fileInfo.Length,
                ContentType = ResolveContentType(fileInfo.Name),
                LastModifiedAtUtc = fileInfo.LastWriteTimeUtc
            });
        }

        return Task.FromResult<IReadOnlyList<FolderItem>>(items);
    }

    /// <inheritdoc />
    public Task CreateFolderAsync(string folderPath, CancellationToken cancellationToken)
    {
        var fullPath = ResolvePath(folderPath);

        Directory.CreateDirectory(fullPath);

        LogFolderCreated(_logger, folderPath);

        return Task.CompletedTask;
    }

    // ─── Helpers ────────────────────────────────────────────────────────

    private string ResolvePath(string relativePath)
    {
        var combined = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var resolved = Path.GetFullPath(combined);

        // Security: ensure resolved path is within base path (prevent directory traversal).
        // Uses platform-aware comparison: case-sensitive on Linux, case-insensitive on Windows/macOS.
        // _basePath always ends with separator so "/storage" cannot match "/storage-other".
        if (!resolved.StartsWith(_basePath, PathComparison))
        {
            throw new UnauthorizedAccessException($"Path traversal detected: {relativePath}");
        }

        return resolved;
    }

    /// <summary>
    /// Returns file creation time. On Linux (ext4), CreationTimeUtc may not be supported
    /// and returns the same as LastWriteTimeUtc — this is expected and correct.
    /// </summary>
    private static DateTime GetCreationTimeUtc(FileInfo fileInfo)
    {
        var created = fileInfo.CreationTimeUtc;
        var modified = fileInfo.LastWriteTimeUtc;

        // On some Linux file systems, creation time returns DateTime.MinValue or a bogus date.
        // Fall back to LastWriteTimeUtc in those cases.
        return created < modified ? created : modified;
    }

    private static string ResolveContentType(string fileName)
    {
        if (!ContentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }

    // ─── Source-generated log messages ──────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Local file deleted: {FilePath}")]
    private static partial void LogFileDeleted(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Local folder created: {FolderPath}")]
    private static partial void LogFolderCreated(ILogger logger, string folderPath);
}
