using CITL.Application.Core.Notifications.Email;
using CITL.Application.Core.Scheduler;
using Microsoft.Extensions.Logging;

namespace CITL.Infrastructure.Core.Scheduler.Jobs;

/// <summary>
/// Scheduled job that sends emails based on the scheduler configuration.
/// Builds a <see cref="SendEmailRequest"/> from the config and delegates to <see cref="IEmailSender"/>.
/// </summary>
internal sealed partial class EmailSchedulerJob(
    IEmailSender emailSender,
    ILogger<EmailSchedulerJob> logger) : IScheduledJob
{
    /// <inheritdoc />
    public string JobType => "EmailJob";

    /// <inheritdoc />
    public async Task ExecuteAsync(SchedulerJobContext context)
    {
        var config = context.Config;

        LogEmailJobStarted(logger, config.SchJobId, config.SchJobName, context.TenantId, config.SchMailTo);

        var request = new SendEmailRequest
        {
            To = config.SchMailTo,
            Cc = config.SchMailCc,
            Bcc = config.SchMailBcc,
            Subject = config.SchMailSubject,
            Body = config.SchMailBody
        };

        var result = await emailSender.SendAsync(request, null, null, context.CancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Error.Description;
            LogEmailJobFailed(logger, config.SchJobId, config.SchJobName, context.TenantId, errorMessage);
            throw new InvalidOperationException(
                $"Email job '{config.SchJobName}' failed: {errorMessage}");
        }

        LogEmailJobCompleted(logger, config.SchJobId, config.SchJobName, context.TenantId);
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email scheduler job started: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', To='{MailTo}'")]
    private static partial void LogEmailJobStarted(ILogger logger, int jobId, string jobName, string tenantId, string mailTo);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email scheduler job completed: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}'")]
    private static partial void LogEmailJobCompleted(ILogger logger, int jobId, string jobName, string tenantId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Email scheduler job failed: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', Error='{ErrorMessage}'")]
    private static partial void LogEmailJobFailed(ILogger logger, int jobId, string jobName, string tenantId, string errorMessage);
}
