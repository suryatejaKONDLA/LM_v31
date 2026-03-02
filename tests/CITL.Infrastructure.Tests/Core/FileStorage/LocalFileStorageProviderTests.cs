using CITL.Infrastructure.Core.FileStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.Tests.Core.FileStorage;

/// <summary>
/// Integration tests for <see cref="LocalFileStorageProvider"/> using a temporary directory.
/// Each test creates its own temp folder for full isolation.
/// </summary>
public sealed class LocalFileStorageProviderTests : IDisposable
{
    private readonly string _tempPath;
    private readonly LocalFileStorageProvider _provider;

    public LocalFileStorageProviderTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "citl_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPath);

        var settings = Options.Create(new FileStorageSettings { Provider = "Local", LocalBasePath = _tempPath });
        var logger = NullLogger<LocalFileStorageProvider>.Instance;
        _provider = new(settings, logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }

    // ── Upload ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_CreatesFileAndReturnsMetadata()
    {
        // Arrange
        var content = "Hello, CITL!"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var metadata = await _provider.UploadAsync(
            "tenant1/docs/test.txt", stream, "text/plain", CancellationToken.None);

        // Assert
        Assert.Equal("test.txt", metadata.FileName);
        Assert.Equal(".txt", metadata.Extension);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.Equal(content.Length, metadata.SizeInBytes);
        Assert.NotEmpty(metadata.Hash);
        Assert.True(File.Exists(Path.Combine(_tempPath, "tenant1", "docs", "test.txt")));
    }

    [Fact]
    public async Task UploadAsync_OverwritesExistingFile()
    {
        // Arrange
        using var stream1 = new MemoryStream("v1"u8.ToArray());
        await _provider.UploadAsync("overwrite.txt", stream1, "text/plain", CancellationToken.None);

        using var stream2 = new MemoryStream("version-two"u8.ToArray());

        // Act
        var metadata = await _provider.UploadAsync("overwrite.txt", stream2, "text/plain", CancellationToken.None);

        // Assert
        Assert.Equal(11, metadata.SizeInBytes);
    }

    // ── Download ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DownloadAsync_ReturnsStreamWithContent()
    {
        // Arrange
        var content = "download-test"u8.ToArray();
        using var uploadStream = new MemoryStream(content);
        await _provider.UploadAsync("dl/file.txt", uploadStream, "text/plain", CancellationToken.None);

        // Act
        using var download = await _provider.DownloadAsync("dl/file.txt", CancellationToken.None);

        // Assert
        Assert.Equal("file.txt", download.FileName);
        Assert.Equal(content.Length, download.SizeInBytes);

        using var reader = new StreamReader(download.Content);
        var body = await reader.ReadToEndAsync(CancellationToken.None);
        Assert.Equal("download-test", body);
    }

    [Fact]
    public async Task DownloadAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _provider.DownloadAsync("nonexistent.txt", CancellationToken.None));
    }

    // ── Exists ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_FileExists_ReturnsTrue()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);
        await _provider.UploadAsync("exists.txt", stream, "text/plain", CancellationToken.None);

        // Act
        var exists = await _provider.ExistsAsync("exists.txt", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_FileDoesNotExist_ReturnsFalse()
    {
        // Act
        var exists = await _provider.ExistsAsync("nope.txt", CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    // ── Metadata ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetadataAsync_FileExists_ReturnsMetadata()
    {
        // Arrange
        using var stream = new MemoryStream("meta-test"u8.ToArray());
        await _provider.UploadAsync("meta/data.json", stream, "application/json", CancellationToken.None);

        // Act
        var metadata = await _provider.GetMetadataAsync("meta/data.json", CancellationToken.None);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("data.json", metadata.FileName);
        Assert.Equal(".json", metadata.Extension);
        Assert.Equal(9, metadata.SizeInBytes);
    }

    [Fact]
    public async Task GetMetadataAsync_FileDoesNotExist_ReturnsNull()
    {
        // Act
        var metadata = await _provider.GetMetadataAsync("nope.json", CancellationToken.None);

        // Assert
        Assert.Null(metadata);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_FileExists_RemovesFile()
    {
        // Arrange
        using var stream = new MemoryStream([1]);
        await _provider.UploadAsync("delete-me.txt", stream, "text/plain", CancellationToken.None);

        // Act
        await _provider.DeleteAsync("delete-me.txt", CancellationToken.None);

        // Assert
        var exists = await _provider.ExistsAsync("delete-me.txt", CancellationToken.None);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_FileDoesNotExist_DoesNotThrow()
    {
        // Act & Assert (no exception)
        await _provider.DeleteAsync("never-existed.txt", CancellationToken.None);
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsFilesAndFolders()
    {
        // Arrange
        using var stream1 = new MemoryStream([1]);
        await _provider.UploadAsync("listtest/a.txt", stream1, "text/plain", CancellationToken.None);

        using var stream2 = new MemoryStream([2]);
        await _provider.UploadAsync("listtest/sub/b.txt", stream2, "text/plain", CancellationToken.None);

        // Act
        var contents = await _provider.ListAsync("listtest", CancellationToken.None);

        // Assert
        Assert.Equal(2, contents.TotalCount); // 1 file + 1 sub-folder
        Assert.Contains(contents.Items, i => i is { Name: "a.txt", IsFolder: false });
        Assert.Contains(contents.Items, i => i is { Name: "sub", IsFolder: true });
    }

    [Fact]
    public async Task ListAsync_EmptyFolder_ReturnsEmpty()
    {
        // Arrange
        await _provider.CreateFolderAsync("emptyfolder", CancellationToken.None);

        // Act
        var contents = await _provider.ListAsync("emptyfolder", CancellationToken.None);

        // Assert
        Assert.Equal(0, contents.TotalCount);
    }

    [Fact]
    public async Task ListAsync_NonExistentFolder_ReturnsEmpty()
    {
        // Act
        var contents = await _provider.ListAsync("nosuchfolder", CancellationToken.None);

        // Assert
        Assert.Equal(0, contents.TotalCount);
    }

    // ── Create Folder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFolderAsync_CreatesDirectory()
    {
        // Act
        await _provider.CreateFolderAsync("newfolder/deep", CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_tempPath, "newfolder", "deep")));
    }

    // ── Signed URL (Local) ────────────────────────────────────────────────────

    [Fact]
    public async Task GetSignedUrlAsync_ReturnsApiBackedUrl()
    {
        // Act
        var result = await _provider.GetSignedUrlAsync("tenant/file.pdf", 30, CancellationToken.None);

        // Assert
        Assert.Contains("/FileStorage/Download", result.Url);
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }

    // ── Path Traversal ────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolvePath_TraversalAttempt_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _provider.ExistsAsync("../../etc/passwd", CancellationToken.None));
    }

    // ── Hash Consistency ──────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_SameContent_ProducesSameHash()
    {
        // Arrange
        var content = "deterministic-hash-test"u8.ToArray();

        using var stream1 = new MemoryStream(content);
        var meta1 = await _provider.UploadAsync("hash1.txt", stream1, "text/plain", CancellationToken.None);

        using var stream2 = new MemoryStream(content);
        var meta2 = await _provider.UploadAsync("hash2.txt", stream2, "text/plain", CancellationToken.None);

        // Assert
        Assert.Equal(meta1.Hash, meta2.Hash);
        Assert.NotEmpty(meta1.Hash);
    }
}
