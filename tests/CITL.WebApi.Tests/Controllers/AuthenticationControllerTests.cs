using System.Security.Claims;
using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Authentication;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Authentication;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="AuthenticationController"/>.
/// Verifies correct delegation to services and HTTP response mapping.
/// </summary>
public sealed class AuthenticationControllerTests
{
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();
    private readonly IIdentityVerificationService _identityService = Substitute.For<IIdentityVerificationService>();
    private readonly ICaptchaService _captchaService = Substitute.For<ICaptchaService>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _tenantContext.TenantId.Returns("T1");
        _controller = new(_authService, _identityService, _captchaService, _tenantContext);
    }

    private void SetupHttpContext(string? bearerToken = null, string? loginUser = null)
    {
        var httpContext = new DefaultHttpContext();

        if (bearerToken is not null)
        {
            httpContext.Request.Headers.Authorization = $"Bearer {bearerToken}";
        }

        if (loginUser is not null)
        {
            var claims = new[] { new Claim(AuthConstants.LoginUserClaimType, loginUser) };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    // ── GenerateCaptchaAsync ─────────────────────────────────────────

    [Fact]
    public async Task GenerateCaptchaAsync_ReturnsOkWithCaptchaResponse()
    {
        // Arrange
        var request = new CaptchaRequest { LoginUser = "admin" };
        var captchaResult = new CaptchaResponse
        {
            CaptchaRequired = true,
            CaptchaId = "cap-id",
            CaptchaImageDark = "base64-dark",
            CaptchaImageLight = "base64-light",
            FailedAttempts = 3
        };
        _captchaService.GenerateAsync(Arg.Any<CaptchaRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(captchaResult));

        // Act
        var actionResult = await _controller.GenerateCaptchaAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<CaptchaResponse>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.True(apiResponse.Data!.CaptchaRequired);
    }

    // ── LoginAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenSuccess_Returns200WithTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "pass",
            LoginIp = "127.0.0.1",
            LoginDevice = "Chrome"
        };
        var loginResponse = new LoginResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            LoginId = 1,
            LoginUser = "admin",
            LoginName = "Administrator",
            Roles = ["Admin"],
            Branches = []
        };
        _authService.LoginAsync(Arg.Any<LoginRequest>(), "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(loginResponse));

        // Act
        var actionResult = await _controller.LoginAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Equal("access-token", apiResponse.Data!.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "wrong",
            LoginIp = "127.0.0.1",
            LoginDevice = "Chrome"
        };
        _authService.LoginAsync(Arg.Any<LoginRequest>(), "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LoginResponse>(new Error("Auth.InvalidCredentials", "Bad credentials.")));

        // Act
        var actionResult = await _controller.LoginAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_PassesTenantIdToService()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "pass",
            LoginIp = "127.0.0.1",
            LoginDevice = "Chrome"
        };
        _authService.LoginAsync(Arg.Any<LoginRequest>(), "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(new LoginResponse
            {
                AccessToken = "tok",
                RefreshToken = "ref",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                LoginId = 1,
                LoginUser = "admin",
                LoginName = "Admin",
                Roles = [],
                Branches = []
            }));

        // Act
        await _controller.LoginAsync(request, CancellationToken.None);

        // Assert
        await _authService.Received(1)
            .LoginAsync(Arg.Any<LoginRequest>(), "T1", Arg.Any<CancellationToken>());
    }

    // ── RefreshTokenAsync ────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "valid-refresh", LoginUser = "admin" };
        _authService.RefreshTokenAsync(Arg.Any<RefreshTokenRequest>(), "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Success(new LoginResponse
            {
                AccessToken = "new-token",
                RefreshToken = "ref",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
                LoginId = 1,
                LoginUser = "admin",
                LoginName = "Admin",
                Roles = [],
                Branches = []
            }));

        // Act
        var actionResult = await _controller.RefreshTokenAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenInvalidToken_Returns400()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "expired", LoginUser = "admin" };
        _authService.RefreshTokenAsync(Arg.Any<RefreshTokenRequest>(), "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<LoginResponse>(new Error("Auth.InvalidRefreshToken", "Expired.")));

        // Act
        var actionResult = await _controller.RefreshTokenAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── LogoutAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ExtractsTokenAndDelegatesToService()
    {
        // Arrange
        SetupHttpContext(bearerToken: "my-jwt-token", loginUser: "admin");
        _authService.LogoutAsync("my-jwt-token", "admin", "T1", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.LogoutAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse>(okResult.Value);
        await _authService.Received(1)
            .LogoutAsync("my-jwt-token", "admin", "T1", Arg.Any<CancellationToken>());
    }

    // ── ForgotPasswordAsync ──────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_WhenSuccess_Returns200WithMessage()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            LoginUser = "admin",
            LoginEmailId = "admin@example.com",
            LoginMobileNo = "1234567890"
        };
        _identityService.ForgotPasswordAsync(Arg.Any<ForgotPasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            LoginUser = "",
            LoginEmailId = "x",
            LoginMobileNo = "1"
        };
        _identityService.ForgotPasswordAsync(Arg.Any<ForgotPasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("LoginUser", "Required.")));

        // Act
        var actionResult = await _controller.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── ResetPasswordAsync ───────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new ResetPasswordRequest { Token = "tok", LoginPassword = "NewPass1" };
        _identityService.ResetPasswordAsync(Arg.Any<ResetPasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
    }

    // ── ResendVerificationAsync ──────────────────────────────────────

    [Fact]
    public async Task ResendVerificationAsync_DelegatesToIdentityService()
    {
        // Arrange
        var request = new ResendVerificationRequest
        {
            LoginUser = "admin",
            LoginEmailId = "admin@example.com",
            LoginMobileNo = "1234567890"
        };
        _identityService.ResendVerificationAsync(Arg.Any<ResendVerificationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _controller.ResendVerificationAsync(request, CancellationToken.None);

        // Assert
        await _identityService.Received(1)
            .ResendVerificationAsync(Arg.Any<ResendVerificationRequest>(), Arg.Any<CancellationToken>());
    }

    // ── VerifyEmailAsync ─────────────────────────────────────────────

    [Fact]
    public async Task VerifyEmailAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new VerifyEmailRequest { Token = "valid-token" };
        _identityService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.VerifyEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new VerifyEmailRequest { Token = "bad" };
        _identityService.VerifyEmailAsync(Arg.Any<VerifyEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(new Error("Auth.InvalidToken", "Invalid.")));

        // Act
        var actionResult = await _controller.VerifyEmailAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }
}
