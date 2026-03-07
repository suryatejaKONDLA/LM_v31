using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Account;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.Account;

/// <summary>
/// Unit tests for <see cref="AccountService"/>.
/// Dependencies: IAccountRepository (mocked), ICurrentUser (mocked), real validators, NullLogger.
/// </summary>
public sealed class AccountServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────

    private static ProfileResponse SampleProfile => new()
    {
        LoginId = 1,
        LoginUser = "admin",
        LoginName = "Admin User",
        LoginBranchCode = 1,
        LoginDesignation = "Manager",
        LoginMobileNo = "9876543210",
        LoginEmailId = "admin@example.com",
        LoginGender = "M",
        LoginEmailVerified = true
    };

    private static ChangePasswordRequest ValidChangePassword => new()
    {
        LoginPasswordOld = "OldPass1",
        LoginPassword = "NewPass1"
    };

    private static UpdateProfileRequest ValidUpdateProfile => new()
    {
        LoginName = "Updated User",
        LoginMobileNo = "9876543210",
        LoginEmailId = "updated@example.com"
    };

    private static SpResult SuccessSpResult => new()
    {
        ResultVal = 1,
        ResultType = "SUCCESS",
        ResultMessage = "Operation successful."
    };

    private static SpResult FailureSpResult => new()
    {
        ResultVal = 0,
        ResultType = "ERROR",
        ResultMessage = "Current password is incorrect."
    };

    private static AccountService CreateService(
        IAccountRepository? repository = null,
        ICurrentUser? currentUser = null)
    {
        repository ??= Substitute.For<IAccountRepository>();
        currentUser ??= CreateCurrentUser();
        return new(
            repository,
            currentUser,
            new ChangePasswordRequestValidator(),
            new UpdateProfileRequestValidator(),
            NullLogger<AccountService>.Instance);
    }

    private static ICurrentUser CreateCurrentUser(int loginId = 1, string loginUser = "admin")
    {
        var user = Substitute.For<ICurrentUser>();
        user.LoginId.Returns(loginId);
        user.LoginUser.Returns(loginUser);
        user.LoginName.Returns("Admin User");
        user.TenantId.Returns("T1");
        user.IsAuthenticated.Returns(true);
        return user;
    }

    // ── GetProfileAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetProfileAsync_WhenProfileExists_ReturnsSuccessWithData()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.GetProfileAsync(1, Arg.Any<CancellationToken>()).Returns(SampleProfile);
        var service = CreateService(repository);

        // Act
        var result = await service.GetProfileAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("admin", result.Value.LoginUser);
        Assert.Equal("Admin User", result.Value.LoginName);
    }

    [Fact]
    public async Task GetProfileAsync_WhenNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.GetProfileAsync(1, Arg.Any<CancellationToken>()).Returns(default(ProfileResponse));
        var service = CreateService(repository);

        // Act
        var result = await service.GetProfileAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProfileAsync_RepositoryCalledWithCorrectLoginId()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.GetProfileAsync(42, Arg.Any<CancellationToken>()).Returns(SampleProfile);
        var service = CreateService(repository, CreateCurrentUser(loginId: 42));

        // Act
        await service.GetProfileAsync(CancellationToken.None);

        // Assert
        await repository.Received(1).GetProfileAsync(42, Arg.Any<CancellationToken>());
    }

    // ── ChangePasswordAsync ──────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_WhenSpSucceeds_ReturnsSuccess()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.ChangePasswordAsync(1, "NewPass1", "OldPass1", Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.ChangePasswordAsync(ValidChangePassword, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_WhenSpFails_ReturnsFailure()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.ChangePasswordAsync(1, "NewPass1", "OldPass1", Arg.Any<CancellationToken>())
            .Returns(FailureSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.ChangePasswordAsync(ValidChangePassword, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Account", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithEmptyOldPassword_ReturnsValidationFailure()
    {
        // Arrange
        var request = new ChangePasswordRequest { LoginPasswordOld = "", LoginPassword = "NewPass1" };
        var repository = Substitute.For<IAccountRepository>();
        var service = CreateService(repository);

        // Act
        var result = await service.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive()
            .ChangePasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WithEmptyNewPassword_ReturnsValidationFailure()
    {
        // Arrange
        var request = new ChangePasswordRequest { LoginPasswordOld = "OldPass1", LoginPassword = "" };
        var repository = Substitute.For<IAccountRepository>();
        var service = CreateService(repository);

        // Act
        var result = await service.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    // ── UpdateProfileAsync ──────────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_WhenSpSucceeds_ReturnsSuccess()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.UpdateProfileAsync(ValidUpdateProfile, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_WhenSpFails_ReturnsFailure()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(FailureSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.UpdateProfileAsync(ValidUpdateProfile, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Account", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithEmptyName_ReturnsValidationFailure()
    {
        // Arrange
        var request = new UpdateProfileRequest { LoginName = "" };
        var repository = Substitute.For<IAccountRepository>();
        var service = CreateService(repository);

        // Act
        var result = await service.UpdateProfileAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive()
            .UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProfileAsync_RepositoryReceivesCorrectLoginId()
    {
        // Arrange
        var repository = Substitute.For<IAccountRepository>();
        repository.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), 42, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);
        var service = CreateService(repository, CreateCurrentUser(loginId: 42));

        // Act
        await service.UpdateProfileAsync(ValidUpdateProfile, CancellationToken.None);

        // Assert
        await repository.Received(1)
            .UpdateProfileAsync(Arg.Any<UpdateProfileRequest>(), 42, Arg.Any<CancellationToken>());
    }
}
