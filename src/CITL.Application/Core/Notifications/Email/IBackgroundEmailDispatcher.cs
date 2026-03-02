namespace CITL.Application.Core.Notifications.Email;

/// <summary>
/// Dispatches emails in the background using a fire-and-forget pattern.
/// Creates a new DI scope with the correct tenant context for each email.
/// </summary>
public interface IBackgroundEmailDispatcher
{
    /// <summary>
    /// Enqueues an email to be sent in a background task.
    /// The email is sent using the default SMTP configuration for the resolved tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to set in the background scope.</param>
    /// <param name="databaseName">The database name to set in the background scope.</param>
    /// <param name="recipientEmail">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlBody">The rendered HTML body.</param>
    /// <param name="inlineImages">Optional inline images to embed via CID.</param>
    void Enqueue(string tenantId, string databaseName, string recipientEmail, string subject, string htmlBody, IReadOnlyList<InlineImage>? inlineImages = null);
}
