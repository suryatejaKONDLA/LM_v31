using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Notifications.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CITL.Infrastructure.Core.Notifications.Email;

/// <summary>
/// Fire-and-forget email dispatcher that creates a new DI scope per email
/// with the correct tenant context. Uses <see cref="IServiceScopeFactory"/> to
/// ensure scoped services (DB connections, tenant context) are properly resolved.
/// </summary>
internal sealed partial class BackgroundEmailDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundEmailDispatcher> logger) : IBackgroundEmailDispatcher
{
    /// <inheritdoc />
    public void Enqueue(string tenantId, string databaseName, string recipientEmail, string subject, string htmlBody, IReadOnlyList<InlineImage>? inlineImages = null)
    {
        LogEmailEnqueued(logger, recipientEmail, subject, tenantId);

        _ = Task.Run(async () =>
        {
            try
            {
                var scope = scopeFactory.CreateAsyncScope();
                await using (scope.ConfigureAwait(false))
                {
                    var provider = scope.ServiceProvider;

                    // Set tenant context in the new scope
                    var tenantContext = provider.GetRequiredService<ITenantContext>();
                    tenantContext.SetTenant(tenantId, databaseName);

                    var emailSender = provider.GetRequiredService<IEmailSender>();

                    var request = new SendEmailRequest
                    {
                        To = recipientEmail,
                        Subject = subject,
                        Body = htmlBody
                    };

                    var result = await emailSender.SendAsync(request, null, inlineImages, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (result.IsSuccess)
                    {
                        LogEmailSent(logger, recipientEmail, subject, tenantId);
                    }
                    else
                    {
                        LogEmailFailed(logger, recipientEmail, subject, tenantId, result.Error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEmailException(logger, ex, recipientEmail, subject, tenantId);
            }
        });
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Background email enqueued — To: '{To}', Subject: '{Subject}', Tenant: '{TenantId}'")]
    private static partial void LogEmailEnqueued(ILogger logger, string to, string subject, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Background email sent — To: '{To}', Subject: '{Subject}', Tenant: '{TenantId}'")]
    private static partial void LogEmailSent(ILogger logger, string to, string subject, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Background email failed — To: '{To}', Subject: '{Subject}', Tenant: '{TenantId}', Error: '{ErrorDescription}'")]
    private static partial void LogEmailFailed(ILogger logger, string to, string subject, string tenantId, string errorDescription);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Background email exception — To: '{To}', Subject: '{Subject}', Tenant: '{TenantId}'")]
    private static partial void LogEmailException(ILogger logger, Exception ex, string to, string subject, string tenantId);
}
