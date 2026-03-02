using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Notifications.Email;

/// <summary>
/// Application service for sending emails.
/// Validates input, then delegates to <see cref="IEmailSender"/> (Infrastructure).
/// </summary>
public sealed partial class EmailService(
    IEmailSender emailSender,
    IValidator<SendEmailRequest> validator,
    ILogger<EmailService> logger) : IEmailService
{
    /// <inheritdoc />
    public async Task<Result> SendAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        return await SendWithAttachmentsAsync(request, null, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result> SendWithAttachmentsAsync(
        SendEmailRequest request,
        IReadOnlyList<EmailAttachment>? attachments,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        LogSendRequested(logger, request.To, request.Subject);

        var result = await emailSender.SendAsync(request, attachments, null, cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            LogSendSucceeded(logger, request.To, request.Subject);
        }
        else
        {
            LogSendFailed(logger, request.To, request.Subject, result.Error.Description);
        }

        return result;
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email send requested — To: '{To}', Subject: '{Subject}'")]
    private static partial void LogSendRequested(ILogger logger, string to, string subject);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email sent successfully — To: '{To}', Subject: '{Subject}'")]
    private static partial void LogSendSucceeded(ILogger logger, string to, string subject);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Email send failed — To: '{To}', Subject: '{Subject}', Error: '{ErrorDescription}'")]
    private static partial void LogSendFailed(ILogger logger, string to, string subject, string errorDescription);
}
