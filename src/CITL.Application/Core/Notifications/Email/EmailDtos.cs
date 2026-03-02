using System.Text.Json.Serialization;

namespace CITL.Application.Core.Notifications.Email;

/// <summary>
/// Request DTO for sending an email.
/// </summary>
public sealed class SendEmailRequest
{
    /// <summary>
    /// Comma-separated recipient email addresses.
    /// </summary>
    [JsonPropertyName("To")]
    public required string To { get; init; }

    /// <summary>
    /// Comma-separated CC email addresses.
    /// </summary>
    [JsonPropertyName("Cc")]
    public string? Cc { get; init; }

    /// <summary>
    /// Comma-separated BCC email addresses.
    /// </summary>
    [JsonPropertyName("Bcc")]
    public string? Bcc { get; init; }

    /// <summary>
    /// Email subject line.
    /// </summary>
    [JsonPropertyName("Subject")]
    public required string Subject { get; init; }

    /// <summary>
    /// HTML email body.
    /// </summary>
    [JsonPropertyName("Body")]
    public required string Body { get; init; }

    /// <summary>
    /// Optional SMTP configuration ID. When null, uses the default active configuration.
    /// </summary>
    [JsonPropertyName("Mail_SNo")]
    public int? MailSNo { get; init; }
}

/// <summary>
/// Represents an email attachment for programmatic use (not API-bound).
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// The file name including extension.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The attachment content stream.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// The MIME content type (e.g., "application/pdf"). Defaults to "application/octet-stream".
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";
}

/// <summary>
/// Represents an inline image embedded in the email body via CID (Content-ID).
/// </summary>
public sealed class InlineImage
{
    /// <summary>
    /// The Content-ID used to reference the image in the HTML body (e.g., "app-logo").
    /// </summary>
    public required string ContentId { get; init; }

    /// <summary>
    /// The MIME type of the image (e.g., "image/jpeg", "image/png").
    /// </summary>
    public required string MimeType { get; init; }

    /// <summary>
    /// The raw image bytes.
    /// </summary>
    public required byte[] Content { get; init; }
}
