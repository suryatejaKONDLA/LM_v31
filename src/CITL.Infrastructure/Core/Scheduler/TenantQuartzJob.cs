using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CITL.Infrastructure.Core.Scheduler;

/// <summary>
/// Quartz <see cref="IJob"/> bridge that creates a new DI scope per execution,
/// resolves the tenant context, and delegates to the appropriate <see cref="IScheduledJob"/>.
/// </summary>
/// <remarks>
/// This class is NOT created by DI — Quartz instantiates it per execution.
/// It receives <see cref="IServiceScopeFactory"/> via constructor injection
/// (Quartz DI integration). All scoped services are resolved from a child scope.
/// </remarks>
internal sealed partial class TenantQuartzJob(
    IServiceScopeFactory scopeFactory,
    ILogger<TenantQuartzJob> logger) : IJob
{
    /// <summary>
    /// JobDataMap key for the tenant identifier.
    /// </summary>
    internal const string TenantIdKey = "TenantId";

    /// <summary>
    /// JobDataMap key for the database name.
    /// </summary>
    internal const string DatabaseNameKey = "DatabaseName";

    /// <summary>
    /// JobDataMap key for the job type string (e.g., "EmailJob").
    /// </summary>
    internal const string JobTypeKey = "JobType";

    /// <summary>
    /// JobDataMap key for the scheduler configuration (serialized as the config object).
    /// </summary>
    internal const string ConfigKey = "Config";

    /// <summary>
    /// JobDataMap key for the timeout in seconds.
    /// </summary>
    internal const string TimeoutSecondsKey = "TimeoutSeconds";

    /// <summary>
    /// JobDataMap key for the retry attempt count.
    /// </summary>
    internal const string RetryAttemptsKey = "RetryAttempts";

    /// <summary>
    /// JobDataMap key for the retry interval in seconds.
    /// </summary>
    internal const string RetryIntervalSecondsKey = "RetryIntervalSeconds";

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        var tenantId = dataMap.GetString(TenantIdKey)!;
        var databaseName = dataMap.GetString(DatabaseNameKey)!;
        var jobType = dataMap.GetString(JobTypeKey)!;
        var config = (SchedulerConfigResponse)dataMap[ConfigKey];
        var timeoutSeconds = dataMap.GetInt(TimeoutSecondsKey);
        var retryAttempts = dataMap.GetInt(RetryAttemptsKey);
        var retryIntervalSeconds = dataMap.GetInt(RetryIntervalSecondsKey);

        LogJobStarted(logger, config.SchJobId, config.SchJobName, tenantId, jobType);

        var attempt = 0;
        var maxAttempts = retryAttempts + 1;
        Exception? lastException = null;

        while (attempt < maxAttempts)
        {
            attempt++;

            try
            {
                await ExecuteSingleAttemptAsync(
                    context, tenantId, databaseName, jobType, config, timeoutSeconds)
                    .ConfigureAwait(false);

                // Success — update tracking
                await UpdateTrackingAsync(tenantId, databaseName, config.SchJobId, context, "Success", null)
                    .ConfigureAwait(false);

                LogJobCompleted(logger, config.SchJobId, config.SchJobName, tenantId, attempt);
                return;
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                LogJobCancelled(logger, config.SchJobId, config.SchJobName, tenantId);
                await UpdateTrackingAsync(tenantId, databaseName, config.SchJobId, context, "Cancelled", "Job was cancelled")
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                LogJobAttemptFailed(logger, ex, config.SchJobId, config.SchJobName, tenantId, attempt, maxAttempts);

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryIntervalSeconds), context.CancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        // All attempts exhausted
        var errorMessage = lastException?.Message ?? "Unknown error after all retry attempts";
        LogJobFailed(logger, config.SchJobId, config.SchJobName, tenantId, maxAttempts, errorMessage);

        await UpdateTrackingAsync(tenantId, databaseName, config.SchJobId, context, "Failed", errorMessage)
            .ConfigureAwait(false);

        throw new JobExecutionException(
            $"Job '{config.SchJobName}' (ID: {config.SchJobId}) failed after {maxAttempts} attempt(s) for tenant '{tenantId}': {errorMessage}",
            lastException!,
            refireImmediately: false);
    }

    private async Task ExecuteSingleAttemptAsync(
        IJobExecutionContext context,
        string tenantId,
        string databaseName,
        string jobType,
        SchedulerConfigResponse config,
        int timeoutSeconds)
    {
        var scope = scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var provider = scope.ServiceProvider;

            // Set tenant context for the new scope
            var tenantContext = provider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenantId, databaseName);

            // Resolve the correct IScheduledJob by matching JobType
            var scheduledJobs = provider.GetServices<IScheduledJob>();
            var job = null as IScheduledJob;

            foreach (var candidate in scheduledJobs)
            {
                if (string.Equals(candidate.JobType, jobType, StringComparison.OrdinalIgnoreCase))
                {
                    job = candidate;
                    break;
                }
            }

            if (job is null)
            {
                throw new InvalidOperationException(
                    $"No IScheduledJob implementation found for job type '{jobType}'. " +
                    "Ensure the job type is registered in DI.");
            }

            // Create a timeout-aware cancellation token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                context.CancellationToken, timeoutCts.Token);

            var jobContext = new SchedulerJobContext
            {
                Config = config,
                TenantId = tenantId,
                CancellationToken = linkedCts.Token
            };

            await job.ExecuteAsync(jobContext).ConfigureAwait(false);
        } // end await using scope
    }

    private async Task UpdateTrackingAsync(
        string tenantId,
        string databaseName,
        int jobId,
        IJobExecutionContext context,
        string status,
        string? errorMessage)
    {
        try
        {
            var scope = scopeFactory.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var provider = scope.ServiceProvider;

                var tenantContext = provider.GetRequiredService<ITenantContext>();
                tenantContext.SetTenant(tenantId, databaseName);

                var repository = provider.GetRequiredService<ISchedulerRepository>();

                var nextFireTime = context.NextFireTimeUtc?.UtcDateTime;

                await repository.UpdateLastRunAsync(
                    jobId,
                    DateTime.UtcNow,
                    nextFireTime,
                    status,
                    errorMessage,
                    CancellationToken.None).ConfigureAwait(false);
            } // end await using scope
        }
        catch (Exception ex)
        {
            LogTrackingUpdateFailed(logger, ex, jobId, tenantId);
        }
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Scheduler job started: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', Type='{JobType}'")]
    private static partial void LogJobStarted(ILogger logger, int jobId, string jobName, string tenantId, string jobType);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Scheduler job completed: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', Attempt={Attempt}")]
    private static partial void LogJobCompleted(ILogger logger, int jobId, string jobName, string tenantId, int attempt);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Scheduler job cancelled: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}'")]
    private static partial void LogJobCancelled(ILogger logger, int jobId, string jobName, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Scheduler job attempt failed: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', Attempt={Attempt}/{MaxAttempts}")]
    private static partial void LogJobAttemptFailed(ILogger logger, Exception exception, int jobId, string jobName, string tenantId, int attempt, int maxAttempts);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Scheduler job failed after all attempts: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', TotalAttempts={TotalAttempts}, Error='{ErrorMessage}'")]
    private static partial void LogJobFailed(ILogger logger, int jobId, string jobName, string tenantId, int totalAttempts, string errorMessage);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to update tracking for JobId={JobId}, Tenant='{TenantId}'")]
    private static partial void LogTrackingUpdateFailed(ILogger logger, Exception exception, int jobId, string tenantId);
}
