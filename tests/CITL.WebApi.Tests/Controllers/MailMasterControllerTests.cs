using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.MailMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Admin;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="MailMasterController"/>.
/// Verifies correct delegation to <see cref="IMailMasterService"/> and HTTP response mapping.
/// </summary>
public sealed class MailMasterControllerTests
{
    private readonly IMailMasterService _service = Substitute.For<IMailMasterService>();
    private readonly MailMasterController _controller;

    public MailMasterControllerTests()
    {
        _controller = new(_service);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Returns200WithItems()
    {
        // Arrange
        IReadOnlyList<MailMasterResponse> items = [new() { MailSNo = 1, MailFromAddress = "test@example.com" }];
        _service.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        // Act
        var actionResult = await _controller.GetAllAsync(true, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<MailMasterResponse>>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Single(apiResponse.Data!);
    }

    // ── GetDropDownAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDropDownAsync_Returns200WithItems()
    {
        // Arrange
        IReadOnlyList<DropDownResponse<int>> items = [new() { Value = 1, Text = "test@example.com" }];
        _service.GetDropDownAsync(true, Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        // Act
        var actionResult = await _controller.GetDropDownAsync(true, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse<IReadOnlyList<DropDownResponse<int>>>>(okResult.Value);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenFound_Returns200()
    {
        // Arrange
        var item = new MailMasterResponse { MailSNo = 1, MailFromAddress = "test@example.com" };
        _service.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Result.Success(item));

        // Act
        var actionResult = await _controller.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<MailMasterResponse>>(okResult.Value);
        Assert.Equal("test@example.com", apiResponse.Data!.MailFromAddress);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _service.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MailMasterResponse>(Error.NotFound("MailMaster", "99")));

        // Act
        var actionResult = await _controller.GetByIdAsync(99, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── AddOrUpdateAsync ─────────────────────────────────────────────

    [Fact]
    public async Task AddOrUpdateAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new MailMasterRequest
        {
            MailSNo = 0,
            MailBranchCode = 1,
            MailFromAddress = "test@example.com",
            MailFromPassword = "pwd",
            MailDisplayName = "Test",
            MailHost = "smtp.test.com",
            MailPort = 587,
            MailSslEnabled = true,
            MailMaxRecipients = 50,
            MailRetryAttempts = 3,
            MailRetryIntervalMinutes = 5,
            MailIsActive = true,
            MailIsDefault = false
        };
        _service.AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse>(okResult.Value);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new MailMasterRequest
        {
            MailSNo = 0,
            MailBranchCode = 1,
            MailFromAddress = "",
            MailFromPassword = "p",
            MailDisplayName = "T",
            MailHost = "h",
            MailPort = 587
        };
        _service.AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("MailFromAddress", "Required.")));

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenSuccess_Returns200()
    {
        // Arrange
        _service.DeleteAsync(1, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.DeleteAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _service.DeleteAsync(99, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("MailMaster", "99")));

        // Act
        var actionResult = await _controller.DeleteAsync(99, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToService()
    {
        // Arrange
        _service.DeleteAsync(5, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.DeleteAsync(5, CancellationToken.None);

        // Assert
        await _service.Received(1).DeleteAsync(5, Arg.Any<CancellationToken>());
    }
}
