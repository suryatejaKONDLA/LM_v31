using CITL.Application.Core.Notifications.Email;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Notifications;

/// <summary>
/// Sends emails via configured SMTP settings.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class EmailController(IEmailService emailService) : CitlControllerBase
{
    /// <summary>
    /// Sends an HTML email using the specified or default SMTP configuration.
    /// </summary>
    /// <param name="request">The email details (To, Subject, Body, optional CC/BCC and Mail_SNo).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Email sent successfully.</response>
    /// <response code="400">Validation failed or SMTP configuration error.</response>
    [HttpPost("Send")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendAsync(
        [FromBody] SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await emailService.SendAsync(request, cancellationToken);
        return FromResult(result, "Email sent successfully.");
    }

    /// <summary>
    /// Sends a test email. Optionally attaches a dummy text file for testing.
    /// </summary>
    /// <param name="request">The email details (To, Subject, Body as plain text).</param>
    /// <param name="attachDummyFile">When <c>true</c>, attaches a sample text file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Test email sent successfully.</response>
    /// <response code="400">Validation failed or SMTP configuration error.</response>
    [HttpPost("SendTest")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendTestAsync(
        [FromBody] SendEmailRequest request,
        [FromQuery] bool attachDummyFile = false,
        CancellationToken cancellationToken = default)
    {
        List<EmailAttachment>? attachments = null;

        if (attachDummyFile)
        {
            var dummyContent = "This is a dummy text file attached for testing purposes.\n"
                             + $"Generated at: {DateTime.UtcNow:O}\n"
                             + "Sent from CITL Email Service.";

            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(dummyContent));
            // Stream ownership transfers to the email service — it handles disposal.
            attachments =
            [
                new()
                {
                    FileName = "test-attachment.txt",
                    Content = stream,
                    ContentType = "text/plain"
                }
            ];
        }

        // Mark body as plain text by wrapping in <pre> so MailKit sends it as-is
        var textRequest = new SendEmailRequest
        {
            To = request.To,
            Cc = request.Cc,
            Bcc = request.Bcc,
            Subject = request.Subject,
            Body = $"<pre>{System.Net.WebUtility.HtmlEncode(request.Body)}</pre>",
            MailSNo = request.MailSNo
        };

        var result = await emailService.SendWithAttachmentsAsync(textRequest, attachments, cancellationToken);
        return FromResult(result, "Test email sent successfully.");
    }
}
