using CITL.Application.Core.Authentication;

namespace CITL.Application.Tests.Core.Authentication;

/// <summary>
/// Unit tests for identity verification validators:
/// <see cref="ForgotPasswordRequestValidator"/>, <see cref="ResetPasswordRequestValidator"/>,
/// <see cref="ResendVerificationRequestValidator"/>, and <see cref="VerifyEmailRequestValidator"/>.
/// </summary>
public sealed class IdentityVerificationValidatorTests
{
    // ═══════════════════════════════════════════════════════════════════════
    // ForgotPasswordRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly ForgotPasswordRequestValidator _forgotValidator = new();

    private static ForgotPasswordRequest CreateForgotRequest(
        string loginUser = "admin",
        string loginEmailId = "admin@example.com",
        string loginMobileNo = "9876543210") => new()
        {
            LoginUser = loginUser,
            LoginEmailId = loginEmailId,
            LoginMobileNo = loginMobileNo
        };

    [Fact]
    public async Task ForgotPassword_WithValidRequest_IsValid()
    {
        var result = await _forgotValidator.ValidateAsync(CreateForgotRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyLoginUser_IsInvalid()
    {
        var request = CreateForgotRequest(loginUser: "");
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginUser");
    }

    [Fact]
    public async Task ForgotPassword_WithLoginUserExceeding100Chars_IsInvalid()
    {
        var request = CreateForgotRequest(loginUser: new string('a', 101));
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_IsInvalid()
    {
        var request = CreateForgotRequest(loginEmailId: "");
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginEmailId");
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_IsInvalid()
    {
        var request = CreateForgotRequest(loginEmailId: "not-an-email");
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginEmailId");
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyMobileNo_IsInvalid()
    {
        var request = CreateForgotRequest(loginMobileNo: "");
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginMobileNo");
    }

    [Fact]
    public async Task ForgotPassword_WithMobileNoExceeding15Chars_IsInvalid()
    {
        var request = CreateForgotRequest(loginMobileNo: new string('9', 16));
        var result = await _forgotValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResetPasswordRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly ResetPasswordRequestValidator _resetValidator = new();

    private static ResetPasswordRequest CreateResetRequest(
        string token = "valid-token-123",
        string loginPassword = "NewP@ss1") => new()
        {
            Token = token,
            LoginPassword = loginPassword
        };

    [Fact]
    public async Task ResetPassword_WithValidRequest_IsValid()
    {
        var result = await _resetValidator.ValidateAsync(CreateResetRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ResetPassword_WithEmptyToken_IsInvalid()
    {
        var request = CreateResetRequest(token: "");
        var result = await _resetValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Token");
    }

    [Fact]
    public async Task ResetPassword_WithTokenExceeding100Chars_IsInvalid()
    {
        var request = CreateResetRequest(token: new string('t', 101));
        var result = await _resetValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ResetPassword_WithEmptyPassword_IsInvalid()
    {
        var request = CreateResetRequest(loginPassword: "");
        var result = await _resetValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginPassword");
    }

    [Fact]
    public async Task ResetPassword_WithPasswordExceeding25Chars_IsInvalid()
    {
        var request = CreateResetRequest(loginPassword: new string('p', 26));
        var result = await _resetValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ResetPassword_WithPasswordBelow6Chars_IsInvalid()
    {
        var request = CreateResetRequest(loginPassword: "abc");
        var result = await _resetValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ResetPassword_WithPasswordExactly6Chars_IsValid()
    {
        var request = CreateResetRequest(loginPassword: "abcdef");
        var result = await _resetValidator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ResendVerificationRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly ResendVerificationRequestValidator _resendValidator = new();

    private static ResendVerificationRequest CreateResendRequest(
        string loginUser = "user1",
        string loginEmailId = "user1@example.com",
        string loginMobileNo = "1234567890") => new()
        {
            LoginUser = loginUser,
            LoginEmailId = loginEmailId,
            LoginMobileNo = loginMobileNo
        };

    [Fact]
    public async Task ResendVerification_WithValidRequest_IsValid()
    {
        var result = await _resendValidator.ValidateAsync(CreateResendRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ResendVerification_WithEmptyLoginUser_IsInvalid()
    {
        var request = CreateResendRequest(loginUser: "");
        var result = await _resendValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ResendVerification_WithInvalidEmail_IsInvalid()
    {
        var request = CreateResendRequest(loginEmailId: "bad-email");
        var result = await _resendValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ResendVerification_WithEmptyMobile_IsInvalid()
    {
        var request = CreateResendRequest(loginMobileNo: "");
        var result = await _resendValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VerifyEmailRequestValidator
    // ═══════════════════════════════════════════════════════════════════════

    private readonly VerifyEmailRequestValidator _verifyValidator = new();

    [Fact]
    public async Task VerifyEmail_WithValidToken_IsValid()
    {
        var request = new VerifyEmailRequest { Token = "valid-token" };
        var result = await _verifyValidator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VerifyEmail_WithEmptyToken_IsInvalid()
    {
        var request = new VerifyEmailRequest { Token = "" };
        var result = await _verifyValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Token");
    }

    [Fact]
    public async Task VerifyEmail_WithTokenExceeding100Chars_IsInvalid()
    {
        var request = new VerifyEmailRequest { Token = new string('t', 101) };
        var result = await _verifyValidator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }
}
