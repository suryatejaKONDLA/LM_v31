using System.Text.Json.Serialization;

namespace CITL.Application.Core.FileStorage;

/// <summary>
/// Metadata about a stored file returned by upload, metadata, and list operations.
/// </summary>
public sealed class StoredFileMetadata
{
    [JsonPropertyName("FileName")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("FilePath")]
    public string FilePath { get; init; } = string.Empty;

    [JsonPropertyName("Extension")]
    public string Extension { get; init; } = string.Empty;

    [JsonPropertyName("ContentType")]
    public string ContentType { get; init; } = string.Empty;

    [JsonPropertyName("SizeInBytes")]
    public long SizeInBytes { get; init; }

    [JsonPropertyName("Hash")]
    public string Hash { get; init; } = string.Empty;

    [JsonPropertyName("CreatedAtUtc")]
    public DateTime CreatedAtUtc { get; init; }

    [JsonPropertyName("LastModifiedAtUtc")]
    public DateTime LastModifiedAtUtc { get; init; }
}

/// <summary>
/// Represents a file download — the stream, content type, and filename.
/// The caller owns and must dispose the <see cref="Content"/> stream.
/// </summary>
public sealed class FileDownloadResult : IDisposable
{
    /// <summary>The raw file content stream.</summary>
    public required Stream Content { get; init; }

    /// <summary>MIME content type (e.g. "application/pdf").</summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>Original filename for the Content-Disposition header.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Total file size in bytes for Content-Length header.</summary>
    public long SizeInBytes { get; init; }

    /// <inheritdoc />
    public void Dispose() => Content.Dispose();
}

/// <summary>
/// Represents a single item in a folder listing — either a file or a sub-folder.
/// </summary>
public sealed class FolderItem
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("IsFolder")]
    public bool IsFolder { get; init; }

    [JsonPropertyName("SizeInBytes")]
    public long SizeInBytes { get; init; }

    [JsonPropertyName("ContentType")]
    public string ContentType { get; init; } = string.Empty;

    [JsonPropertyName("LastModifiedAtUtc")]
    public DateTime LastModifiedAtUtc { get; init; }
}

/// <summary>
/// Contents of a folder — its items and total count.
/// </summary>
public sealed class FolderContents
{
    [JsonPropertyName("FolderPath")]
    public string FolderPath { get; init; } = string.Empty;

    [JsonPropertyName("Items")]
    public IReadOnlyList<FolderItem> Items { get; init; } = [];

    [JsonPropertyName("TotalCount")]
    public int TotalCount { get; init; }
}

/// <summary>
/// Request body for downloading all files from one or more folders as a ZIP archive.
/// </summary>
public sealed class ZipFolderRequest
{
    /// <summary>Folder paths to include. Use <c>/</c> for the tenant root.</summary>
    [JsonPropertyName("FolderPaths")]
    public IReadOnlyList<string> FolderPaths { get; init; } = [];
}

/// <summary>
/// Request body for downloading specific files as a ZIP archive.
/// </summary>
public sealed class ZipFilesRequest
{
    /// <summary>List of file paths to include in the ZIP.</summary>
    [JsonPropertyName("FilePaths")]
    public IReadOnlyList<string> FilePaths { get; init; } = [];
}

/// <summary>
/// A single node in a file tree — either a file with a downloadable path, or a folder with children.
/// </summary>
public sealed class FileTreeNode
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Relative path usable with the Download endpoint. Empty for folders.</summary>
    [JsonPropertyName("Path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("IsFolder")]
    public bool IsFolder { get; init; }

    [JsonPropertyName("SizeInBytes")]
    public long SizeInBytes { get; init; }

    [JsonPropertyName("ContentType")]
    public string ContentType { get; init; } = string.Empty;

    [JsonPropertyName("LastModifiedAtUtc")]
    public DateTime LastModifiedAtUtc { get; init; }

    [JsonPropertyName("Children")]
    public List<FileTreeNode> Children { get; init; } = [];
}

/// <summary>
/// Recursive tree view of all files and folders under a root path.
/// </summary>
public sealed class FileTree
{
    [JsonPropertyName("RootPath")]
    public string RootPath { get; init; } = string.Empty;

    [JsonPropertyName("Nodes")]
    public IReadOnlyList<FileTreeNode> Nodes { get; init; } = [];

    [JsonPropertyName("TotalFiles")]
    public int TotalFiles { get; init; }

    [JsonPropertyName("TotalFolders")]
    public int TotalFolders { get; init; }

    [JsonPropertyName("TotalSizeInBytes")]
    public long TotalSizeInBytes { get; init; }
}

/// <summary>
/// A pre-signed URL for direct download (R2) or an API-backed URL (Local).
/// </summary>
public sealed class SignedUrlResult
{
    [JsonPropertyName("Url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("ExpiresAtUtc")]
    public DateTime ExpiresAtUtc { get; init; }
}
