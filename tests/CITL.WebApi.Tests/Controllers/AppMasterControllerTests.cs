using CITL.Application.Core.Admin.AppMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Admin;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="AppMasterController"/>.
/// Verifies correct delegation to service and HTTP response mapping.
/// </summary>
public sealed class AppMasterControllerTests
{
    private readonly IAppMasterService _service = Substitute.For<IAppMasterService>();
    private readonly AppMasterController _controller;

    public AppMasterControllerTests()
    {
        _controller = new(_service);
    }

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenServiceReturnsSuccess_Returns200WithData()
    {
        // Arrange
        var response = new AppMasterResponse
        {
            AppCode = 1,
            AppHeader1 = "CITL",
            AppHeader2 = "CT",
            AppLink = "https://www.citl.co.in/"
        };
        _service.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var actionResult = await _controller.GetAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<AppMasterResponse>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Equal("CITL", apiResponse.Data!.AppHeader1);
    }

    [Fact]
    public async Task GetAsync_WhenServiceReturnsNotFound_Returns404()
    {
        // Arrange
        _service.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppMasterResponse>(Error.NotFound("AppMaster", "config")));

        // Act
        var actionResult = await _controller.GetAsync(CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAsync_DelegatesToService()
    {
        // Arrange
        _service.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppMasterResponse>(Error.NotFound("AppMaster", "x")));

        // Act
        await _controller.GetAsync(CancellationToken.None);

        // Assert
        await _service.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    // ── AddOrUpdateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task AddOrUpdateAsync_WhenServiceReturnsSuccess_Returns200WithMessage()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "Test",
            AppHeader2 = "TST",
            SessionId = 1,
            BranchCode = 1
        };
        _service.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WhenServiceReturnsFailure_Returns400()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "Test",
            AppHeader2 = "TST",
            SessionId = 1,
            BranchCode = 1
        };
        _service.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("AppHeader1", "Required.")));

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task AddOrUpdateAsync_DelegatesToService()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "X",
            AppHeader2 = "Y",
            SessionId = 1,
            BranchCode = 1
        };
        _service.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        await _service.Received(1).AddOrUpdateAsync(
            Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>());
    }
}
