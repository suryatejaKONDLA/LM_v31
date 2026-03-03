using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Admin.AppMaster;
using CITL.Application.Core.Authentication;
using CITL.Application.Core.Notifications.Email;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.Authentication;

/// <summary>
/// Unit tests for <see cref="IdentityVerificationService"/>.
/// </summary>
public sealed class IdentityVerificationServiceTests
{
    private readonly IIdentityVerificationRepository _repository = Substitute.For<IIdentityVerificationRepository>();
    private readonly IAppMasterRepository _appMasterRepository = Substitute.For<IAppMasterRepository>();
    private readonly IBackgroundEmailDispatcher _emailDispatcher = Substitute.For<IBackgroundEmailDispatcher>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IdentityVerificationService _service;

    public IdentityVerificationServiceTests()
    {
        _tenantContext.TenantId.Returns("T1");
        _tenantContext.DatabaseName.Returns("CITLPOS");

        _service = new(
            _repository,
            _appMasterRepository,
            _emailDispatcher,
            _tenantContext,
            new ForgotPasswordRequestValidator(),
            new ResetPasswordRequestValidator(),
            new ResendVerificationRequestValidator(),
            new VerifyEmailRequestValidator(),
            NullLogger<IdentityVerificationService>.Instance);
    }

    private static UserIdentityInfo SampleUser => new()
    {
        LoginId = 42,
        LoginName = "John Doe",
        LoginEmailId = "john@example.com",
        LoginEmailVerifiedFlag = false
    };

    private static AppMasterResponse SampleAppMaster => new()
    {
        AppCode = 1,
        AppHeader1 = "CITL",
        AppHeader2 = "CT",
        AppLink = "https://app.citl.co.in"
    };

    // ═══════════════════════════════════════════════════════════════════════
    // ForgotPasswordAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ForgotPasswordAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange — empty LoginUser
        var request = new ForgotPasswordRequest
        {
            LoginUser = "",
            LoginEmailId = "e@e.com",
            LoginMobileNo = "123"
        };

        // Act
        var result = await _service.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ReturnsSuccess()
    {
        // Arrange — user not found but still returns success (security)
        var request = new ForgotPasswordRequest
        {
            LoginUser = "unknown",
            LoginEmailId = "unknown@example.com",
            LoginMobileNo = "1234567890"
        };
        _repository.VerifyUserIdentityAsync("unknown", "unknown@example.com", "1234567890", Arg.Any<CancellationToken>())
            .Returns(default(UserIdentityInfo?));

        // Act
        var result = await _service.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _repository.DidNotReceive().UpsertTokenAsync(
            Arg.Any<int>(), Arg.Any<byte>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserFound_CreatesTokenAndEnqueuesEmail()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            LoginUser = "admin",
            LoginEmailId = "john@example.com",
            LoginMobileNo = "9876543210"
        };
        _repository.VerifyUserIdentityAsync("admin", "john@example.com", "9876543210", Arg.Any<CancellationToken>())
            .Returns(SampleUser);
        _appMasterRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(SampleAppMaster);

        // Act
        var result = await _service.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _repository.Received(1).UpsertTokenAsync(
            42, 2, Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        _emailDispatcher.Received(1).Enqueue(
            "T1", "CITLPOS", "john@example.com", Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<InlineImage>?>());
    }

    [Fact]
    public async Task ForgotPasswordAsync_AppLinkNotConfigured_ReturnsFailure()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            LoginUser = "admin",
            LoginEmailId = "john@example.com",
            LoginMobileNo = "9876543210"
        };
        _repository.VerifyUserIdentityAsync("admin", "john@example.com", "9876543210", Arg.Any<CancellationToken>())
            .Returns(SampleUser);
        _appMasterRepository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppMasterResponse { AppCode = 1, AppHeader1 = "CITL", AppHeader2 = "CT", AppLink = "" });

        // Act
        var result = await _service.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("AppLinkNotConfigured", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResetPasswordAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ResetPasswordAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange — password too short
        var request = new ResetPasswordRequest { Token = "tok", LoginPassword = "ab" };

        // Act
        var result = await _service.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var request = new ResetPasswordRequest { Token = "expired-token", LoginPassword = "NewPass1" };
        _repository.ValidateTokenAsync(2, "expired-token", Arg.Any<CancellationToken>())
            .Returns(default(int?));

        // Act
        var result = await _service.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("InvalidToken", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ResetsPasswordAndDeletesToken()
    {
        // Arrange
        var request = new ResetPasswordRequest { Token = "valid-token", LoginPassword = "NewPass1" };
        _repository.ValidateTokenAsync(2, "valid-token", Arg.Any<CancellationToken>())
            .Returns(42);

        // Act
        var result = await _service.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _repository.Received(1).ResetPasswordAsync(42, "NewPass1", Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteTokenAsync(42, 2, Arg.Any<CancellationToken>());
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResendVerificationAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ResendVerificationAsync_ValidationFails_ReturnsFailure()
    {
        var request = new ResendVerificationRequest
        {
            LoginUser = "",
            LoginEmailId = "e@e.com",
            LoginMobileNo = "123"
        };

        var result = await _service.ResendVerificationAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ResendVerificationAsync_UserNotFound_ReturnsSuccess()
    {
        var request = new ResendVerificationRequest
        {
            LoginUser = "unknown",
            LoginEmailId = "u@example.com",
            LoginMobileNo = "1234567890"
        };
        _repository.VerifyUserIdentityAsync("unknown", "u@example.com", "1234567890", Arg.Any<CancellationToken>())
            .Returns(default(UserIdentityInfo?));

        var result = await _service.ResendVerificationAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ResendVerificationAsync_AlreadyVerified_ReturnsFailure()
    {
        var request = new ResendVerificationRequest
        {
            LoginUser = "admin",
            LoginEmailId = "john@example.com",
            LoginMobileNo = "9876543210"
        };
        var verifiedUser = new UserIdentityInfo
        {
            LoginId = 42,
            LoginName = "John",
            LoginEmailId = "john@example.com",
            LoginEmailVerifiedFlag = true
        };
        _repository.VerifyUserIdentityAsync("admin", "john@example.com", "9876543210", Arg.Any<CancellationToken>())
            .Returns(verifiedUser);

        var result = await _service.ResendVerificationAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("AlreadyVerified", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResendVerificationAsync_ValidUnverifiedUser_EnqueuesEmail()
    {
        var request = new ResendVerificationRequest
        {
            LoginUser = "admin",
            LoginEmailId = "john@example.com",
            LoginMobileNo = "9876543210"
        };
        _repository.VerifyUserIdentityAsync("admin", "john@example.com", "9876543210", Arg.Any<CancellationToken>())
            .Returns(SampleUser);
        _appMasterRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(SampleAppMaster);

        var result = await _service.ResendVerificationAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).UpsertTokenAsync(
            42, 1, Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        _emailDispatcher.Received(1).Enqueue(
            "T1", "CITLPOS", "john@example.com", Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<InlineImage>?>());
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VerifyEmailAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task VerifyEmailAsync_ValidationFails_ReturnsFailure()
    {
        var request = new VerifyEmailRequest { Token = "" };
        var result = await _service.VerifyEmailAsync(request, CancellationToken.None);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFailure()
    {
        var request = new VerifyEmailRequest { Token = "bad-token" };
        _repository.ValidateTokenAsync(1, "bad-token", Arg.Any<CancellationToken>())
            .Returns(default(int?));

        var result = await _service.VerifyEmailAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("InvalidToken", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_SetsVerifiedAndDeletesToken()
    {
        var request = new VerifyEmailRequest { Token = "valid-token" };
        _repository.ValidateTokenAsync(1, "valid-token", Arg.Any<CancellationToken>())
            .Returns(42);

        var result = await _service.VerifyEmailAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).SetEmailVerifiedAsync(42, Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteTokenAsync(42, 1, Arg.Any<CancellationToken>());
    }
}
