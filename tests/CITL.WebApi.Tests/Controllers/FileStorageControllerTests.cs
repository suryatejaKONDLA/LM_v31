using CITL.Application.Core.FileStorage;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.FileStorage;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="FileStorageController"/>.
/// Verifies correct delegation to <see cref="IFileStorageService"/> and HTTP response mapping.
/// </summary>
public sealed class FileStorageControllerTests
{
    private readonly IFileStorageService _service = Substitute.For<IFileStorageService>();
    private readonly FileStorageController _controller;

    public FileStorageControllerTests()
    {
        _controller = new(_service)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    // ── Upload ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_ValidFile_Returns200WithMetadata()
    {
        // Arrange
        var file = CreateMockFormFile("test.pdf", "application/pdf", 1024);

        _service.UploadAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new StoredFileMetadata
            {
                FileName = "test.pdf",
                FilePath = "invoices/test.pdf",
                Extension = ".pdf",
                ContentType = "application/pdf",
                SizeInBytes = 1024
            }));

        // Act
        var actionResult = await _controller.UploadAsync(file, "invoices", cancellationToken: CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<StoredFileMetadata>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Equal("test.pdf", apiResponse.Data!.FileName);
    }

    [Fact]
    public async Task UploadAsync_NullFile_Returns400()
    {
        // Act
        var actionResult = await _controller.UploadAsync(null!, cancellationToken: CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("No file provided", apiResponse.Message);
    }

    [Fact]
    public async Task UploadAsync_EmptyFile_Returns400()
    {
        // Arrange
        var file = CreateMockFormFile("empty.txt", "text/plain", 0);

        // Act
        var actionResult = await _controller.UploadAsync(file, cancellationToken: CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task UploadAsync_ServiceFailure_ReturnsBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("bad.txt", "text/plain", 10);

        _service.UploadAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<StoredFileMetadata>(
                Error.Validation("FileStorage.InvalidPath", "Path contains invalid characters.")));

        // Act
        var actionResult = await _controller.UploadAsync(file, "../evil", cancellationToken: CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── Download ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DownloadAsync_FileExists_ReturnsFileStreamResult()
    {
        // Arrange
        _service.DownloadAsync("docs/file.pdf", Arg.Any<CancellationToken>())
            .Returns(Result.Success(new FileDownloadResult
            {
                Content = new MemoryStream([1, 2, 3]),
                ContentType = "application/pdf",
                FileName = "file.pdf",
                SizeInBytes = 3
            }));

        // Act
        var actionResult = await _controller.DownloadAsync("docs/file.pdf", CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(actionResult);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("file.pdf", fileResult.FileDownloadName);
        Assert.True(fileResult.EnableRangeProcessing);
    }

    [Fact]
    public async Task DownloadAsync_FileNotFound_Returns404()
    {
        // Arrange
        _service.DownloadAsync("nope.pdf", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<FileDownloadResult>(
                Error.NotFound("FileStorage.FileNotFound", "nope.pdf")));

        // Act
        var actionResult = await _controller.DownloadAsync("nope.pdf", CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── Metadata ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMetadataAsync_FileExists_Returns200()
    {
        // Arrange
        _service.GetMetadataAsync("docs/readme.md", Arg.Any<CancellationToken>())
            .Returns(Result.Success(new StoredFileMetadata
            {
                FileName = "readme.md",
                FilePath = "docs/readme.md",
                SizeInBytes = 256
            }));

        // Act
        var actionResult = await _controller.GetMetadataAsync("docs/readme.md", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMetadataAsync_FileNotFound_Returns404()
    {
        // Arrange
        _service.GetMetadataAsync("missing.txt", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<StoredFileMetadata>(
                Error.NotFound("FileStorage.FileNotFound", "missing.txt")));

        // Act
        var actionResult = await _controller.GetMetadataAsync("missing.txt", CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── List ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsOkWithContents()
    {
        // Arrange
        _service.ListAsync("invoices", Arg.Any<CancellationToken>())
            .Returns(Result.Success(new FolderContents
            {
                FolderPath = "invoices",
                Items = [new() { Name = "a.pdf", IsFolder = false }],
                TotalCount = 1
            }));

        // Act
        var actionResult = await _controller.ListAsync("invoices", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(okResult.Value);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_FileExists_Returns200()
    {
        // Arrange
        _service.DeleteAsync("old.txt", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.DeleteAsync("old.txt", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task DeleteAsync_FileNotFound_Returns404()
    {
        // Arrange
        _service.DeleteAsync("gone.txt", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(
                Error.NotFound("FileStorage.FileNotFound", "gone.txt")));

        // Act
        var actionResult = await _controller.DeleteAsync("gone.txt", CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── Create Folder ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFolderAsync_ValidPath_Returns200()
    {
        // Arrange
        _service.CreateFolderAsync("new-folder", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.CreateFolderAsync("new-folder", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Contains("Folder created", apiResponse.Message);
    }

    // ── Signed URL ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSignedUrlAsync_FileExists_Returns200()
    {
        // Arrange
        _service.GetSignedUrlAsync("docs/file.pdf", 15, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SignedUrlResult
            {
                Url = "https://r2.example.com/signed",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            }));

        // Act
        var actionResult = await _controller.GetSignedUrlAsync("docs/file.pdf", 15, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(okResult.Value);
    }

    // ── Delegation Verification ───────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_DelegatesToService()
    {
        // Arrange
        var file = CreateMockFormFile("delegate.txt", "text/plain", 5);

        _service.UploadAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new StoredFileMetadata { FileName = "delegate.txt" }));

        // Act
        await _controller.UploadAsync(file, "uploads", cancellationToken: CancellationToken.None);

        // Assert
        await _service.Received(1).UploadAsync(
            "uploads", "delegate.txt", Arg.Any<Stream>(), "text/plain", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToService()
    {
        // Arrange
        _service.DeleteAsync("target.txt", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.DeleteAsync("target.txt", CancellationToken.None);

        // Assert
        await _service.Received(1).DeleteAsync("target.txt", Arg.Any<CancellationToken>());
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var stream = new MemoryStream(new byte[length]);
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.OpenReadStream().Returns(stream);

        return file;
    }
}
