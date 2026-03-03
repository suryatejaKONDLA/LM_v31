using CITL.Application.Common.Interfaces;
using CITL.Application.Core.FileStorage;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.FileStorage;

/// <summary>
/// Unit tests for <see cref="FileStorageService"/>.
/// Dependencies: IFileStorageProvider (mocked), ITenantContext (mocked), NullLogger.
/// </summary>
public sealed class FileStorageServiceTests
{
    private const string TenantId = "CITLPOS";

    private static FileStorageService CreateService(
        IFileStorageProvider? provider = null,
        ITenantContext? tenantContext = null,
        FileStorageUploadSettings? uploadSettings = null)
    {
        provider ??= Substitute.For<IFileStorageProvider>();

        tenantContext ??= CreateTenantContext();

        uploadSettings ??= new();

        var logger = NullLogger<FileStorageService>.Instance;
        return new(provider, tenantContext, uploadSettings, logger);
    }

    private static ITenantContext CreateTenantContext()
    {
        var ctx = Substitute.For<ITenantContext>();
        ctx.TenantId.Returns(TenantId);
        return ctx;
    }

    // ── Upload ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_ValidFile_ReturnsSuccessWithMetadata()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new StoredFileMetadata
            {
                FileName = "report.pdf",
                FilePath = "CITLPOS/invoices/report.pdf",
                Extension = ".pdf",
                ContentType = "application/pdf",
                SizeInBytes = 1024,
                Hash = "ABC123",
                CreatedAtUtc = DateTime.UtcNow,
                LastModifiedAtUtc = DateTime.UtcNow
            });

        var service = CreateService(provider);
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var result = await service.UploadAsync("invoices", "report.pdf", stream, "application/pdf", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("report.pdf", result.Value.FileName);
        Assert.Equal("invoices/report.pdf", result.Value.FilePath);
        Assert.Equal(1024, result.Value.SizeInBytes);

        await provider.Received(1).UploadAsync(
            "CITLPOS/invoices/report.pdf",
            Arg.Any<Stream>(),
            "application/pdf",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadAsync_EmptyFileName_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var result = await service.UploadAsync("invoices", "", stream, "application/pdf", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("File name is required", result.Error.Description);
    }

    [Fact]
    public async Task UploadAsync_PathTraversal_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var result = await service.UploadAsync("../etc", "passwd", stream, "text/plain", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("invalid characters", result.Error.Description);
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();

        // Create a seekable stream that reports > 1GB
        using var stream = new FakeLargeStream(1_073_741_825);

        // Act
        var result = await service.UploadAsync("uploads", "large.bin", stream, "application/octet-stream", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("1 GB size limit", result.Error.Description);
    }

    // ── Download ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DownloadAsync_FileExists_ReturnsStream()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync("CITLPOS/invoices/report.pdf", Arg.Any<CancellationToken>())
            .Returns(true);

        provider.DownloadAsync("CITLPOS/invoices/report.pdf", Arg.Any<CancellationToken>())
            .Returns(new FileDownloadResult
            {
                Content = new MemoryStream([1, 2, 3]),
                ContentType = "application/pdf",
                FileName = "report.pdf",
                SizeInBytes = 3
            });

        var service = CreateService(provider);

        // Act
        var result = await service.DownloadAsync("invoices/report.pdf", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("report.pdf", result.Value.FileName);
        result.Value.Dispose();
    }

    [Fact]
    public async Task DownloadAsync_FileNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var service = CreateService(provider);

        // Act
        var result = await service.DownloadAsync("missing.pdf", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task DownloadAsync_EmptyPath_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.DownloadAsync("", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("PathRequired", result.Error.Code);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_FileExists_ReturnsSuccess()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync("CITLPOS/reports/old.pdf", Arg.Any<CancellationToken>())
            .Returns(true);

        var service = CreateService(provider);

        // Act
        var result = await service.DeleteAsync("reports/old.pdf", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await provider.Received(1).DeleteAsync("CITLPOS/reports/old.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_FileNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var service = CreateService(provider);

        // Act
        var result = await service.DeleteAsync("nope.pdf", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("NotFound", result.Error.Code);
    }

    // ── Metadata ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetadataAsync_FileExists_ReturnsMetadata()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.GetMetadataAsync("CITLPOS/docs/readme.md", Arg.Any<CancellationToken>())
            .Returns(new StoredFileMetadata
            {
                FileName = "readme.md",
                FilePath = "CITLPOS/docs/readme.md",
                Extension = ".md",
                ContentType = "text/markdown",
                SizeInBytes = 512,
                Hash = "DEF456"
            });

        var service = CreateService(provider);

        // Act
        var result = await service.GetMetadataAsync("docs/readme.md", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("readme.md", result.Value.FileName);
        Assert.Equal("docs/readme.md", result.Value.FilePath);
    }

    [Fact]
    public async Task GetMetadataAsync_FileNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(StoredFileMetadata?));

        var service = CreateService(provider);

        // Act
        var result = await service.GetMetadataAsync("missing.txt", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("NotFound", result.Error.Code);
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsFolderContents()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ListAsync("CITLPOS/invoices", Arg.Any<CancellationToken>())
            .Returns(new FolderContents
            {
                FolderPath = "CITLPOS/invoices",
                Items =
                [
                    new()
                    {
                        Name = "report.pdf",
                        Path = "CITLPOS/invoices/report.pdf",
                        IsFolder = false,
                        SizeInBytes = 1024,
                        ContentType = "application/pdf"
                    }
                ],
                TotalCount = 1
            });

        var service = CreateService(provider);

        // Act
        var result = await service.ListAsync("invoices", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("invoices/report.pdf", result.Value.Items[0].Path);
    }

    // ── Create Folder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFolderAsync_ValidPath_ReturnsSuccess()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        var service = CreateService(provider);

        // Act
        var result = await service.CreateFolderAsync("invoices/2026", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await provider.Received(1).CreateFolderAsync("CITLPOS/invoices/2026", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFolderAsync_EmptyPath_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.CreateFolderAsync("", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("FolderPathRequired", result.Error.Code);
    }

    // ── Signed URL ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSignedUrlAsync_FileExists_ReturnsUrl()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync("CITLPOS/docs/file.pdf", Arg.Any<CancellationToken>())
            .Returns(true);

        provider.GetSignedUrlAsync("CITLPOS/docs/file.pdf", 15, Arg.Any<CancellationToken>())
            .Returns(new SignedUrlResult
            {
                Url = "https://r2.example.com/signed-url",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            });

        var service = CreateService(provider);

        // Act
        var result = await service.GetSignedUrlAsync("docs/file.pdf", 15, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("signed-url", result.Value.Url);
    }

    [Fact]
    public async Task GetSignedUrlAsync_InvalidExpiry_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetSignedUrlAsync("docs/file.pdf", 0, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("InvalidExpiry", result.Error.Code);
    }

    // ── ZIP Download ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DownloadFolderAsZipAsync_EmptyPaths_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();
        using var outputStream = new MemoryStream();

        // Act
        var result = await service.DownloadFolderAsZipAsync([], outputStream, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("ZipRequestEmpty", result.Error.Code);
    }

    [Fact]
    public async Task DownloadFolderAsZipAsync_RootSlash_ListsFromTenantRoot()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ListAllRecursiveAsync("CITLPOS", Arg.Any<CancellationToken>())
            .Returns(
            [
                new() { Path = "CITLPOS/readme.txt", Name = "readme.txt", IsFolder = false }
            ]);
        provider.ExistsAsync("CITLPOS/readme.txt", Arg.Any<CancellationToken>()).Returns(true);
        provider.DownloadAsync("CITLPOS/readme.txt", Arg.Any<CancellationToken>())
            .Returns(new FileDownloadResult
            {
                Content = new MemoryStream("hi"u8.ToArray()),
                ContentType = "text/plain",
                FileName = "readme.txt",
                SizeInBytes = 2
            });

        var service = CreateService(provider);
        using var outputStream = new MemoryStream();

        // Act
        var result = await service.DownloadFolderAsZipAsync(["/"], outputStream, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(outputStream.Length > 0);
    }

    [Fact]
    public async Task DownloadFilesAsZipAsync_EmptyPaths_ReturnsValidationFailure()
    {
        // Arrange
        var service = CreateService();
        using var outputStream = new MemoryStream();

        // Act
        var result = await service.DownloadFilesAsZipAsync([], outputStream, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("ZipRequestEmpty", result.Error.Code);
    }

    [Fact]
    public async Task DownloadFilesAsZipAsync_WithFilePaths_WritesZipToStream()
    {
        // Arrange
        var provider = Substitute.For<IFileStorageProvider>();
        provider.ExistsAsync("CITLPOS/docs/a.txt", Arg.Any<CancellationToken>()).Returns(true);
        provider.DownloadAsync("CITLPOS/docs/a.txt", Arg.Any<CancellationToken>())
            .Returns(new FileDownloadResult
            {
                Content = new MemoryStream("hello"u8.ToArray()),
                ContentType = "text/plain",
                FileName = "a.txt",
                SizeInBytes = 5
            });

        var service = CreateService(provider);
        using var outputStream = new MemoryStream();

        // Act
        var result = await service.DownloadFilesAsZipAsync(["docs/a.txt"], outputStream, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(outputStream.Length > 0);
    }

    // ── Path Security ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("folder/../../secret")]
    [InlineData("C:\\Windows\\System32")]
    public async Task UploadAsync_PathTraversalVariants_ReturnsFailure(string folder)
    {
        // Arrange
        var service = CreateService();
        using var stream = new MemoryStream([1]);

        // Act
        var result = await service.UploadAsync(folder, "file.txt", stream, "text/plain", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UploadAsync_NullByteInPath_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();
        using var stream = new MemoryStream([1]);

        // Act
        var result = await service.UploadAsync("uploads\0evil", "file.txt", stream, "text/plain", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// A fake stream that reports a specific length without allocating memory.
    /// </summary>
    private sealed class FakeLargeStream(long length) : MemoryStream
    {
        public override long Length => length;
        public override bool CanSeek => true;
    }
}
