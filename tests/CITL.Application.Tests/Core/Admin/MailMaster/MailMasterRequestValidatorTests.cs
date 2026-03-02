using CITL.Application.Core.Admin.MailMaster;

namespace CITL.Application.Tests.Core.Admin.MailMaster;

/// <summary>
/// Unit tests for <see cref="MailMasterRequestValidator"/>.
/// </summary>
public sealed class MailMasterRequestValidatorTests
{
    private readonly MailMasterRequestValidator _validator = new();

    private static MailMasterRequest CreateRequest(
        int mailSNo = 0,
        int mailBranchCode = 1,
        string mailFromAddress = "noreply@example.com",
        string mailFromPassword = "secret123",
        string mailDisplayName = "CITL Notifications",
        string mailHost = "smtp.example.com",
        int mailPort = 587,
        int mailMaxRecipients = 50,
        int mailRetryAttempts = 3,
        int mailRetryIntervalMinutes = 5) => new()
        {
            MailSNo = mailSNo,
            MailBranchCode = mailBranchCode,
            MailFromAddress = mailFromAddress,
            MailFromPassword = mailFromPassword,
            MailDisplayName = mailDisplayName,
            MailHost = mailHost,
            MailPort = mailPort,
            MailMaxRecipients = mailMaxRecipients,
            MailRetryAttempts = mailRetryAttempts,
            MailRetryIntervalMinutes = mailRetryIntervalMinutes
        };

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        var result = await _validator.ValidateAsync(CreateRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroBranchCode_IsInvalid()
    {
        var request = CreateRequest(mailBranchCode: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MailBranchCode");
    }

    [Fact]
    public async Task Validate_WithEmptyFromAddress_IsInvalid()
    {
        var request = CreateRequest(mailFromAddress: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithInvalidFromAddress_IsInvalid()
    {
        var request = CreateRequest(mailFromAddress: "not-an-email");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithFromAddressExceeding100Chars_IsInvalid()
    {
        var request = CreateRequest(mailFromAddress: new string('a', 90) + "@example.com");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_IsInvalid()
    {
        var request = CreateRequest(mailFromPassword: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithPasswordExceeding256Chars_IsInvalid()
    {
        var request = CreateRequest(mailFromPassword: new string('p', 257));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyDisplayName_IsInvalid()
    {
        var request = CreateRequest(mailDisplayName: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithDisplayNameExceeding40Chars_IsInvalid()
    {
        var request = CreateRequest(mailDisplayName: new string('n', 41));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyHost_IsInvalid()
    {
        var request = CreateRequest(mailHost: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithHostExceeding40Chars_IsInvalid()
    {
        var request = CreateRequest(mailHost: new string('h', 41));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroPort_IsInvalid()
    {
        var request = CreateRequest(mailPort: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithPortExceeding65535_IsInvalid()
    {
        var request = CreateRequest(mailPort: 65536);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(443)]
    [InlineData(587)]
    [InlineData(65535)]
    public async Task Validate_WithValidPort_IsValid(int port)
    {
        var request = CreateRequest(mailPort: port);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroMaxRecipients_IsInvalid()
    {
        var request = CreateRequest(mailMaxRecipients: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNegativeRetryAttempts_IsInvalid()
    {
        var request = CreateRequest(mailRetryAttempts: -1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNegativeRetryInterval_IsInvalid()
    {
        var request = CreateRequest(mailRetryIntervalMinutes: -1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroRetryAttempts_IsValid()
    {
        var request = CreateRequest(mailRetryAttempts: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }
}
