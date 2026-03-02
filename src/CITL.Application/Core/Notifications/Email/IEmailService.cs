using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Notifications.Email;

/// <summary>
/// Application service interface for sending emails with validation.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Validates and sends an email using the specified or default SMTP configuration.
    /// </summary>
    Task<Result> SendAsync(SendEmailRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Validates and sends an email with optional file attachments.
    /// Used programmatically by background services (e.g., scheduler).
    /// </summary>
    Task<Result> SendWithAttachmentsAsync(
        SendEmailRequest request,
        IReadOnlyList<EmailAttachment>? attachments,
        CancellationToken cancellationToken);
}

/// <summary>
/// Infrastructure interface for raw SMTP email sending (no validation).
/// Defined in Application layer; implemented in Infrastructure with MailKit.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email via SMTP. No input validation — caller must validate first.
    /// </summary>
    Task<Result> SendAsync(
        SendEmailRequest request,
        IReadOnlyList<EmailAttachment>? attachments,
        IReadOnlyList<InlineImage>? inlineImages,
        CancellationToken cancellationToken);
}
