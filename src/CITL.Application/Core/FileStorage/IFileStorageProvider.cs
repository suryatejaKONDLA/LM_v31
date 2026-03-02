namespace CITL.Application.Core.FileStorage;

/// <summary>
/// Storage provider abstraction — implemented by Local disk and Cloudflare R2.
/// Providers are tenant-unaware; the service layer handles tenant path prefixing.
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// Uploads a file at the specified path, streaming content from <paramref name="content"/>.
    /// Creates parent directories/prefixes automatically.
    /// </summary>
    /// <param name="path">Relative path including filename (e.g. "invoices/report.pdf").</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Metadata of the uploaded file.</returns>
    Task<StoredFileMetadata> UploadAsync(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Downloads a file as a stream for the specified path.
    /// </summary>
    /// <param name="path">Relative path including filename.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A download result with stream, content type, and size.</returns>
    Task<FileDownloadResult> DownloadAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">Relative path including filename.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a file exists at the specified path.
    /// </summary>
    /// <param name="path">Relative path including filename.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the file exists.</returns>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Gets metadata for a file at the specified path.
    /// </summary>
    /// <param name="path">Relative path including filename.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>File metadata, or <c>null</c> if the file does not exist.</returns>
    Task<StoredFileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Lists files and sub-folders in the specified folder.
    /// </summary>
    /// <param name="folderPath">Relative folder path (e.g. "invoices/2026").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Folder contents.</returns>
    Task<FolderContents> ListAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    /// Generates a pre-signed URL for direct download.
    /// For R2 this returns a Cloudflare URL; for Local this returns an API-backed URL.
    /// </summary>
    /// <param name="path">Relative path including filename.</param>
    /// <param name="expiryMinutes">How long the URL is valid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The signed URL result with expiry.</returns>
    Task<SignedUrlResult> GetSignedUrlAsync(string path, int expiryMinutes, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all files recursively under the specified folder (no delimiter).
    /// Returns a flat list — tree building is handled by the service layer.
    /// </summary>
    /// <param name="folderPath">Relative folder path to list recursively.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Flat list of all files under the folder.</returns>
    Task<IReadOnlyList<FolderItem>> ListAllRecursiveAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a folder at the specified path.
    /// </summary>
    /// <param name="folderPath">Relative folder path to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CreateFolderAsync(string folderPath, CancellationToken cancellationToken);
}
