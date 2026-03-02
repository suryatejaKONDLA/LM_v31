using CITL.Application.Core.Account;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Account;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="AccountController"/>.
/// Verifies correct delegation to <see cref="IAccountService"/> and HTTP response mapping.
/// </summary>
public sealed class AccountControllerTests
{
    private readonly IAccountService _service = Substitute.For<IAccountService>();
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _controller = new(_service);
    }

    // ── GetProfileAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetProfileAsync_WhenSuccess_Returns200WithData()
    {
        // Arrange
        var profile = new ProfileResponse
        {
            LoginId = 1,
            LoginUser = "admin",
            LoginName = "Admin User",
            LoginEmailId = "admin@example.com"
        };
        _service.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(profile));

        // Act
        var actionResult = await _controller.GetProfileAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<ProfileResponse>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Equal("admin", apiResponse.Data!.LoginUser);
    }

    [Fact]
    public async Task GetProfileAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _service.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ProfileResponse>(Error.NotFound("Account.Profile", "admin")));

        // Act
        var actionResult = await _controller.GetProfileAsync(CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetProfileAsync_DelegatesToService()
    {
        // Arrange
        _service.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ProfileResponse>(Error.NotFound("Account.Profile", "x")));

        // Act
        await _controller.GetProfileAsync(CancellationToken.None);

        // Assert
        await _service.Received(1).GetProfileAsync(Arg.Any<CancellationToken>());
    }

    // ── UpdateProfileAsync ──────────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new UpdateProfileRequest { LoginName = "Updated" };
        _service.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new UpdateProfileRequest { LoginName = "X" };
        _service.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("LoginName", "Required.")));

        // Act
        var actionResult = await _controller.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── ChangePasswordAsync ──────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new ChangePasswordRequest { LoginPasswordOld = "old", LoginPassword = "new" };
        _service.ChangePasswordAsync(Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new ChangePasswordRequest { LoginPasswordOld = "old", LoginPassword = "new" };
        _service.ChangePasswordAsync(Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(new Error("Account.ChangePasswordFailed", "Incorrect password.")));

        // Act
        var actionResult = await _controller.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_DelegatesToService()
    {
        // Arrange
        var request = new ChangePasswordRequest { LoginPasswordOld = "old", LoginPassword = "new" };
        _service.ChangePasswordAsync(Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        await _service.Received(1)
            .ChangePasswordAsync(Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>());
    }
}
