using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Authentication;
using CITL.SharedKernel.Results;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CITL.Application.Tests.Core.Authentication;

/// <summary>
/// Unit tests for <see cref="AuthenticationService"/>.
/// Covers login, refresh, and logout flows with all dependency combinations.
/// </summary>
public sealed class AuthenticationServiceTests
{
    private const string TenantId = "CITLPOS";
    private const string TestUser = "admin";
    private const string TestPassword = "P@ssw0rd";
    private const string TestAccessToken = "eyJhbGci.test.token";
    private const string TestRefreshToken = "dGVzdC1yZWZyZXNoLXRva2Vu";
    private const int TestLoginId = 42;
    private const string TestLoginName = "Admin User";

    private readonly IAuthenticationRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly ICaptchaService _captchaService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _authRepository = Substitute.For<IAuthenticationRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _captchaService = Substitute.For<ICaptchaService>();
        _loginValidator = Substitute.For<IValidator<LoginRequest>>();
        _logger = Substitute.For<ILogger<AuthenticationService>>();

        _sut = new AuthenticationService(
            _authRepository,
            _tokenService,
            _captchaService,
            _loginValidator,
            _logger);
    }

    private static LoginRequest CreateLoginRequest(
        string? captchaId = null,
        string? captchaValue = null) => new()
        {
            LoginUser = TestUser,
            LoginPassword = TestPassword,
            CaptchaId = captchaId,
            CaptchaValue = captchaValue,
        };

    private static UserProfile CreateUserProfile() => new()
    {
        LoginId = TestLoginId,
        LoginUser = TestUser,
        LoginName = TestLoginName,
    };

    private void SetupValidValidation()
    {
        _loginValidator
            .ValidateAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupNoCaptchaRequired()
    {
        _captchaService
            .IsCaptchaRequiredAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private void SetupSuccessfulLogin()
    {
        _authRepository
            .LoginCheckAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SpResult { ResultVal = 1, ResultType = "SUCCESS", ResultMessage = "Login successful" });
    }

    private void SetupUserProfile()
    {
        _authRepository
            .GetUserProfileAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(CreateUserProfile());
    }

    private void SetupRolesAndBranches()
    {
        _authRepository
            .GetUserRolesAsync(TestLoginId, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Admin", "User" });

        _authRepository
            .GetUserBranchesAsync(TestLoginId, Arg.Any<CancellationToken>())
            .Returns(new List<BranchInfo>
            {
                new() { BranchCode = 1, BranchName = "Headquarters" }
            });
    }

    private void SetupTokenGeneration()
    {
        _tokenService
            .GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId)
            .Returns(TestAccessToken);

        _tokenService
            .GenerateRefreshToken()
            .Returns(TestRefreshToken);
    }

    // ── LoginAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange
        var errors = new List<ValidationFailure>
        {
            new("LoginUser", "Username is required"),
        };

        _loginValidator
            .ValidateAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(errors));

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        await _authRepository.DidNotReceive()
            .LoginCheckAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_CaptchaRequired_NoCaptchaProvided_ReturnsFailure()
    {
        // Arrange
        SetupValidValidation();
        _captchaService
            .IsCaptchaRequiredAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("CAPTCHA", result.Error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_CaptchaRequired_InvalidCaptcha_ReturnsFailure()
    {
        // Arrange
        SetupValidValidation();
        _captchaService
            .IsCaptchaRequiredAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(true);
        _captchaService
            .ValidateAsync("captcha-123", "WRONG", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("Captcha", "Invalid CAPTCHA.")));

        var request = CreateLoginRequest(captchaId: "captcha-123", captchaValue: "WRONG");

        // Act
        var result = await _sut.LoginAsync(request, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        SetupValidValidation();
        SetupNoCaptchaRequired();

        _authRepository
            .LoginCheckAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SpResult { ResultVal = -1, ResultType = "ERROR", ResultMessage = "Invalid credentials" });

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid credentials", result.Error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_UserProfileNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidValidation();
        SetupNoCaptchaRequired();
        SetupSuccessfulLogin();

        _authRepository
            .GetUserProfileAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(default(UserProfile?));

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ReturnsTokensAndProfile()
    {
        // Arrange
        SetupValidValidation();
        SetupNoCaptchaRequired();
        SetupSuccessfulLogin();
        SetupUserProfile();
        SetupRolesAndBranches();
        SetupTokenGeneration();

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TestAccessToken, result.Value.AccessToken);
        Assert.Equal(TestRefreshToken, result.Value.RefreshToken);
        Assert.Equal(TestLoginId, result.Value.LoginId);
        Assert.Equal(TestUser, result.Value.LoginUser);
        Assert.Equal(TestLoginName, result.Value.LoginName);
        Assert.Equal(2, result.Value.Roles.Count);
        Assert.Single(result.Value.Branches);
        Assert.False(result.Value.MustChangePassword);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_StoresRefreshToken()
    {
        // Arrange
        SetupValidValidation();
        SetupNoCaptchaRequired();
        SetupSuccessfulLogin();
        SetupUserProfile();
        SetupRolesAndBranches();
        SetupTokenGeneration();

        // Act
        await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        await _tokenService.Received(1)
            .StoreRefreshTokenAsync(TestUser, TestRefreshToken, TenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_PasswordResetRequired_SetsMustChangePassword()
    {
        // Arrange
        SetupValidValidation();
        SetupNoCaptchaRequired();

        _authRepository
            .LoginCheckAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SpResult { ResultVal = 2, ResultType = "SUCCESS", ResultMessage = "Password reset required" });

        SetupUserProfile();
        SetupRolesAndBranches();
        SetupTokenGeneration();

        // Act
        var result = await _sut.LoginAsync(CreateLoginRequest(), TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.MustChangePassword);
    }

    [Fact]
    public async Task LoginAsync_CaptchaValid_ProceedsWithLogin()
    {
        // Arrange
        SetupValidValidation();
        _captchaService
            .IsCaptchaRequiredAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(true);
        _captchaService
            .ValidateAsync("captcha-123", "ABC123", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        SetupSuccessfulLogin();
        SetupUserProfile();
        SetupRolesAndBranches();
        SetupTokenGeneration();

        var request = CreateLoginRequest(captchaId: "captcha-123", captchaValue: "ABC123");

        // Act
        var result = await _sut.LoginAsync(request, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ── RefreshTokenAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_InvalidRefreshToken_ReturnsFailure()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            LoginUser = TestUser,
            RefreshToken = "invalid-token",
        };

        _tokenService
            .ValidateRefreshTokenAsync(TestUser, "invalid-token", TenantId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Validation("RefreshToken", "Invalid refresh token.")));

        // Act
        var result = await _sut.RefreshTokenAsync(request, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.RefreshToken", result.Error.Code);
    }

    [Fact]
    public async Task RefreshTokenAsync_UserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            LoginUser = TestUser,
            RefreshToken = TestRefreshToken,
        };

        _tokenService
            .ValidateRefreshTokenAsync(TestUser, TestRefreshToken, TenantId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _authRepository
            .GetUserProfileAsync(TestUser, Arg.Any<CancellationToken>())
            .Returns(default(UserProfile?));

        // Act
        var result = await _sut.RefreshTokenAsync(request, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokenPair()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            LoginUser = TestUser,
            RefreshToken = TestRefreshToken,
        };

        _tokenService
            .ValidateRefreshTokenAsync(TestUser, TestRefreshToken, TenantId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        SetupUserProfile();
        SetupRolesAndBranches();

        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        _tokenService.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId)
            .Returns(newAccessToken);
        _tokenService.GenerateRefreshToken().Returns(newRefreshToken);

        // Act
        var result = await _sut.RefreshTokenAsync(request, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newAccessToken, result.Value.AccessToken);
        Assert.Equal(newRefreshToken, result.Value.RefreshToken);
        Assert.False(result.Value.MustChangePassword);
    }

    [Fact]
    public async Task RefreshTokenAsync_StoresNewRefreshToken()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            LoginUser = TestUser,
            RefreshToken = TestRefreshToken,
        };

        _tokenService
            .ValidateRefreshTokenAsync(TestUser, TestRefreshToken, TenantId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        SetupUserProfile();
        SetupRolesAndBranches();

        var newRefreshToken = "rotated-token";
        _tokenService.GenerateAccessToken(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("access");
        _tokenService.GenerateRefreshToken().Returns(newRefreshToken);

        // Act
        await _sut.RefreshTokenAsync(request, TenantId, CancellationToken.None);

        // Assert
        await _tokenService.Received(1)
            .StoreRefreshTokenAsync(TestUser, newRefreshToken, TenantId, Arg.Any<CancellationToken>());
    }

    // ── LogoutAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_BlacklistsAccessToken()
    {
        // Arrange
        _tokenService.GenerateRefreshToken().Returns("revoked-placeholder");

        // Act
        var result = await _sut.LogoutAsync(TestAccessToken, TestUser, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _tokenService.Received(1)
            .BlacklistTokenAsync(TestAccessToken, TenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_InvalidatesRefreshToken()
    {
        // Arrange
        var newRandomToken = "random-revocation-token";
        _tokenService.GenerateRefreshToken().Returns(newRandomToken);

        // Act
        await _sut.LogoutAsync(TestAccessToken, TestUser, TenantId, CancellationToken.None);

        // Assert — stores a new random token, effectively revoking the old one
        await _tokenService.Received(1)
            .StoreRefreshTokenAsync(TestUser, newRandomToken, TenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_ReturnsSuccess()
    {
        // Arrange
        _tokenService.GenerateRefreshToken().Returns("placeholder");

        // Act
        var result = await _sut.LogoutAsync(TestAccessToken, TestUser, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
