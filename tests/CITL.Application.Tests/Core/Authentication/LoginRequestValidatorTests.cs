using CITL.Application.Core.Authentication;

namespace CITL.Application.Tests.Core.Authentication;

/// <summary>
/// Unit tests for <see cref="LoginRequestValidator"/>.
/// </summary>
public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    private static LoginRequest ValidRequest => new()
    {
        LoginUser = "admin",
        LoginPassword = "P@ssw0rd"
    };

    // ── LoginUser ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        // Act
        var result = await _validator.ValidateAsync(ValidRequest);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyLoginUser_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest { LoginUser = "", LoginPassword = "P@ssw0rd" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginUser");
    }

    [Fact]
    public async Task Validate_WithLoginUserExceeding50Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest { LoginUser = new string('a', 51), LoginPassword = "P@ssw0rd" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginUser");
    }

    // ── LoginPassword ─────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithEmptyLoginPassword_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest { LoginUser = "admin", LoginPassword = "" };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginPassword");
    }

    [Fact]
    public async Task Validate_WithPasswordExceeding128Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest { LoginUser = "admin", LoginPassword = new string('x', 129) };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginPassword");
    }

    // ── LoginIp ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithIpExceeding45Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            LoginIp = new string('1', 46)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginIp");
    }

    // ── LoginDevice ───────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithDeviceExceeding255Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            LoginDevice = new string('d', 256)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LoginDevice");
    }

    // ── CAPTCHA conditional rules ─────────────────────────────────────────

    [Fact]
    public async Task Validate_WithCaptchaIdButNoCaptchaValue_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            CaptchaId = "abc123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CaptchaValue");
    }

    [Fact]
    public async Task Validate_WithCaptchaValueButNoCaptchaId_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            CaptchaValue = "XYZ123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CaptchaId");
    }

    [Fact]
    public async Task Validate_WithBothCaptchaFields_IsValid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            CaptchaId = "abc123",
            CaptchaValue = "XYZ123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNeitherCaptchaField_IsValid()
    {
        // Act
        var result = await _validator.ValidateAsync(ValidRequest);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithCaptchaValueExceeding20Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            CaptchaId = "abc123",
            CaptchaValue = new string('x', 21)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CaptchaValue");
    }

    [Fact]
    public async Task Validate_WithCaptchaIdExceeding50Chars_IsInvalid()
    {
        // Arrange
        var request = new LoginRequest
        {
            LoginUser = "admin",
            LoginPassword = "P@ssw0rd",
            CaptchaId = new string('x', 51),
            CaptchaValue = "XYZ123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CaptchaId");
    }
}
