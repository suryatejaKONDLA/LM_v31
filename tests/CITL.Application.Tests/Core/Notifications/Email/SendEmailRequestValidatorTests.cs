using CITL.Application.Core.Notifications.Email;

namespace CITL.Application.Tests.Core.Notifications.Email;

/// <summary>
/// Unit tests for <see cref="SendEmailRequestValidator"/>.
/// </summary>
public sealed class SendEmailRequestValidatorTests
{
    private readonly SendEmailRequestValidator _validator = new();

    private static SendEmailRequest CreateRequest(
        string to = "user@example.com",
        string subject = "Test Subject",
        string body = "<p>Hello World</p>",
        string? cc = null,
        string? bcc = null,
        int? mailSNo = null) => new()
        {
            To = to,
            Subject = subject,
            Body = body,
            Cc = cc,
            Bcc = bcc,
            MailSNo = mailSNo
        };

    // ── To ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        var result = await _validator.ValidateAsync(CreateRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyTo_IsInvalid()
    {
        var request = CreateRequest(to: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "To");
    }

    [Fact]
    public async Task Validate_WithInvalidToEmail_IsInvalid()
    {
        var request = CreateRequest(to: "not-an-email");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithMultipleValidToEmails_IsValid()
    {
        var request = CreateRequest(to: "a@example.com, b@example.com; c@example.com");
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithToExceeding1000Chars_IsInvalid()
    {
        var request = CreateRequest(to: new string('a', 995) + "@test.com");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ── Subject ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithEmptySubject_IsInvalid()
    {
        var request = CreateRequest(subject: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Subject");
    }

    [Fact]
    public async Task Validate_WithSubjectExceeding200Chars_IsInvalid()
    {
        var request = CreateRequest(subject: new string('s', 201));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ── Body ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithEmptyBody_IsInvalid()
    {
        var request = CreateRequest(body: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Body");
    }

    [Fact]
    public async Task Validate_WithBodyExceeding50000Chars_IsInvalid()
    {
        var request = CreateRequest(body: new string('b', 50001));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ── Cc ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidCc_IsValid()
    {
        var request = CreateRequest(cc: "cc@example.com");
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithInvalidCcEmail_IsInvalid()
    {
        var request = CreateRequest(cc: "bad-email");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNullCc_IsValid()
    {
        var request = CreateRequest(cc: null);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    // ── Bcc ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidBcc_IsValid()
    {
        var request = CreateRequest(bcc: "bcc@example.com");
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithInvalidBccEmail_IsInvalid()
    {
        var request = CreateRequest(bcc: "bad-email");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    // ── MailSNo ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithNullMailSNo_IsValid()
    {
        var request = CreateRequest(mailSNo: null);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithPositiveMailSNo_IsValid()
    {
        var request = CreateRequest(mailSNo: 5);
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroMailSNo_IsInvalid()
    {
        var request = CreateRequest(mailSNo: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithNegativeMailSNo_IsInvalid()
    {
        var request = CreateRequest(mailSNo: -1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }
}
