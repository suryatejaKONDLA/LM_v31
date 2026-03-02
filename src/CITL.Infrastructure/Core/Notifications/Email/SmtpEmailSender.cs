using CITL.Application.Core.Admin.MailMaster;
using CITL.Application.Core.Notifications.Email;
using CITL.SharedKernel.Results;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CITL.Infrastructure.Core.Notifications.Email;

/// <summary>
/// Sends emails via SMTP using MailKit.
/// Reads SMTP configuration from <c>citl_sys.Mail_Master</c>.
/// </summary>
internal sealed partial class SmtpEmailSender(
    IMailMasterRepository repository,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public async Task<Result> SendAsync(
        SendEmailRequest request,
        IReadOnlyList<EmailAttachment>? attachments,
        IReadOnlyList<InlineImage>? inlineImages,
        CancellationToken cancellationToken)
    {
        var smtpConfig = await repository.GetSmtpConfigAsync(request.MailSNo, cancellationToken)
            .ConfigureAwait(false);

        if (smtpConfig is null)
        {
            LogSmtpConfigNotFound(logger, request.MailSNo);
            return Result.Failure(new(
                "Email.SmtpConfigNotFound",
                request.MailSNo.HasValue
                    ? $"SMTP configuration with ID '{request.MailSNo}' was not found."
                    : "No default active SMTP configuration found. Please configure one in Mail Master."));
        }

        LogSendStarted(logger, request.To, request.Subject, smtpConfig.MailFromAddress);

        var message = BuildMessage(smtpConfig, request, attachments, inlineImages);

        try
        {
            using var client = new SmtpClient();

            var secureSocketOptions = smtpConfig.MailSslEnabled
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(
                smtpConfig.MailHost,
                smtpConfig.MailPort,
                secureSocketOptions,
                cancellationToken).ConfigureAwait(false);

            await client.AuthenticateAsync(
                smtpConfig.MailFromAddress,
                smtpConfig.MailFromPassword,
                cancellationToken).ConfigureAwait(false);

            await client.SendAsync(message, cancellationToken).ConfigureAwait(false);

            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            LogSendSucceeded(logger, request.To, request.Subject);

            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogSendFailed(logger, ex, request.To, request.Subject);
            return Result.Failure(new("Email.SendFailed", $"Failed to send email: {ex.Message}"));
        }
    }

    private static MimeMessage BuildMessage(
        SmtpConfig smtpConfig,
        SendEmailRequest request,
        IReadOnlyList<EmailAttachment>? attachments,
        IReadOnlyList<InlineImage>? inlineImages)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(smtpConfig.MailDisplayName, smtpConfig.MailFromAddress));

        AddAddresses(message.To, request.To);

        if (!string.IsNullOrWhiteSpace(request.Cc))
        {
            AddAddresses(message.Cc, request.Cc);
        }

        if (!string.IsNullOrWhiteSpace(request.Bcc))
        {
            AddAddresses(message.Bcc, request.Bcc);
        }

        message.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.Body
        };

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                var contentType = ContentType.Parse(attachment.ContentType);
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, contentType);
            }
        }

        if (inlineImages is not null)
        {
            foreach (var image in inlineImages)
            {
                var mimeType = ContentType.Parse(image.MimeType);
                var resource = bodyBuilder.LinkedResources.Add(image.ContentId, image.Content, mimeType);
                resource.ContentId = image.ContentId;
                resource.ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Inline);
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    private static void AddAddresses(InternetAddressList addressList, string addresses)
    {
        var parts = addresses.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            addressList.Add(MailboxAddress.Parse(part));
        }
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Sending email to '{To}' with subject '{Subject}' via SMTP '{FromAddress}'")]
    private static partial void LogSendStarted(ILogger logger, string to, string subject, string fromAddress);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email sent successfully to '{To}' with subject '{Subject}'")]
    private static partial void LogSendSucceeded(ILogger logger, string to, string subject);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "SMTP configuration not found for Mail_SNo: {MailSNo}")]
    private static partial void LogSmtpConfigNotFound(ILogger logger, int? mailSNo);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to send email to '{To}' with subject '{Subject}'")]
    private static partial void LogSendFailed(ILogger logger, Exception exception, string to, string subject);
}
