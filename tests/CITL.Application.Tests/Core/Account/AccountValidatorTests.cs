using CITL.Application.Core.Account;

namespace CITL.Application.Tests.Core.Account;

/// <summary>
/// Unit tests for <see cref="ChangePasswordRequestValidator"/> and <see cref="UpdateProfileRequestValidator"/>.
/// </summary>
public sealed class AccountValidatorTests
{
    // ═══════════════════════════════════════════════════════════════════════
    // ChangePasswordRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly ChangePasswordRequestValidator _changePasswordValidator = new();

    private static ChangePasswordRequest ValidChangePasswordRequest => new()
    {
        LoginPasswordOld = "OldPass1",
        LoginPassword = "NewPass1"
    };

    [Fact]
    public async Task ChangePassword_WithValidRequest_IsValid()
    {
        var result = await _changePasswordValidator.ValidateAsync(ValidChangePasswordRequest);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ChangePassword_WithEmptyOldPassword_IsInvalid()
    {
        var request = new ChangePasswordRequest { LoginPasswordOld = "", LoginPassword = "NewPass1" };
        var result = await _changePasswordValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginPasswordOld");
    }

    [Fact]
    public async Task ChangePassword_WithOldPasswordExceeding25Chars_IsInvalid()
    {
        var request = new ChangePasswordRequest { LoginPasswordOld = new string('o', 26), LoginPassword = "NewPass1" };
        var result = await _changePasswordValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ChangePassword_WithEmptyNewPassword_IsInvalid()
    {
        var request = new ChangePasswordRequest { LoginPasswordOld = "OldPass1", LoginPassword = "" };
        var result = await _changePasswordValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginPassword");
    }

    [Fact]
    public async Task ChangePassword_WithNewPasswordExceeding25Chars_IsInvalid()
    {
        var request = new ChangePasswordRequest { LoginPasswordOld = "OldPass1", LoginPassword = new string('n', 26) };
        var result = await _changePasswordValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UpdateProfileRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly UpdateProfileRequestValidator _updateProfileValidator = new();

    private static UpdateProfileRequest ValidUpdateProfileRequest => new()
    {
        LoginName = "John Doe"
    };

    [Fact]
    public async Task UpdateProfile_WithValidRequest_IsValid()
    {
        var result = await _updateProfileValidator.ValidateAsync(ValidUpdateProfileRequest);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyLoginName_IsInvalid()
    {
        var request = new UpdateProfileRequest { LoginName = "" };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginName");
    }

    [Fact]
    public async Task UpdateProfile_WithLoginNameExceeding40Chars_IsInvalid()
    {
        var request = new UpdateProfileRequest { LoginName = new string('a', 41) };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithMobileExceeding15Chars_IsInvalid()
    {
        var request = new UpdateProfileRequest { LoginName = "John", LoginMobileNo = new string('9', 16) };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidEmail_IsInvalid()
    {
        var request = new UpdateProfileRequest { LoginName = "John", LoginEmailId = "not-email" };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithValidEmail_IsValid()
    {
        var request = new UpdateProfileRequest { LoginName = "John", LoginEmailId = "john@example.com" };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyEmail_IsValid()
    {
        // Empty email is allowed (optional field)
        var request = new UpdateProfileRequest { LoginName = "John", LoginEmailId = "" };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfile_WithMenuIdExceeding8Chars_IsInvalid()
    {
        var request = new UpdateProfileRequest { LoginName = "John", MenuId = "123456789" };
        var result = await _updateProfileValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }
}
