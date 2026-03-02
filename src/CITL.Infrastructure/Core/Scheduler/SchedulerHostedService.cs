using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Scheduler;
using CITL.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;

namespace CITL.Infrastructure.Core.Scheduler;

/// <summary>
/// Hosted service that manages the Quartz scheduler lifecycle.
/// Loads jobs from all tenant databases on startup and exposes <see cref="ISchedulerAdmin"/>
/// for runtime health monitoring and control.
/// </summary>
internal sealed partial class SchedulerHostedService(
    ISchedulerFactory schedulerFactory,
    IServiceScopeFactory scopeFactory,
    ITenantRegistry tenantRegistry,
    ILogger<SchedulerHostedService> logger) : IHostedService, ISchedulerAdmin
{
    private IScheduler? _scheduler;

    // ── IHostedService ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogSchedulerStarting(logger);

        _scheduler = await schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);

        var tenantIds = tenantRegistry.GetAllTenantIds();

        foreach (var tenantId in tenantIds)
        {
            try
            {
                await LoadTenantJobsAsync(tenantId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogTenantLoadFailed(logger, ex, tenantId);
            }
        }

        await _scheduler.Start(cancellationToken).ConfigureAwait(false);

        LogSchedulerStarted(logger, tenantIds.Count);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler is not null)
        {
            LogSchedulerStopping(logger);
            await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken).ConfigureAwait(false);
            LogSchedulerStopped(logger);
        }
    }

    // ── ISchedulerAdmin — Health & Status ────────────────────────────────────

    /// <inheritdoc />
    public async Task<TenantSchedulerStatusResponse> GetTenantStatusAsync(
        string tenantId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        return await GetTenantStatusInternalAsync(scheduler, tenantId, cancellationToken)
            .ConfigureAwait(false);
    }

    // ── ISchedulerAdmin — Per-Job Operations ─────────────────────────────────

    /// <inheritdoc />
    public async Task PauseJobAsync(string tenantId, int jobId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var jobKey = BuildJobKey(tenantId, jobId);

        await scheduler.PauseJob(jobKey, cancellationToken).ConfigureAwait(false);
        LogJobPaused(logger, jobId, tenantId);
    }

    /// <inheritdoc />
    public async Task ResumeJobAsync(string tenantId, int jobId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var jobKey = BuildJobKey(tenantId, jobId);

        await scheduler.ResumeJob(jobKey, cancellationToken).ConfigureAwait(false);
        LogJobResumed(logger, jobId, tenantId);
    }

    /// <inheritdoc />
    public async Task<Result> TriggerJobAsync(string tenantId, int jobId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var jobKey = BuildJobKey(tenantId, jobId);

        // Guard: reject if the job doesn't exist, is paused, or is stopped
        var stateResult = await GetJobTriggerStateAsync(scheduler, jobKey, cancellationToken)
            .ConfigureAwait(false);

        if (!stateResult.IsSuccess)
        {
            return stateResult;
        }

        var state = stateResult.Value;

        if (state is TriggerState.Paused)
        {
            return Result.Failure(new(
                "Scheduler.JobPaused",
                $"Job {jobId} is paused. Resume it before triggering."));
        }

        if (state is TriggerState.None)
        {
            return Result.Failure(new(
                "Scheduler.JobStopped",
                $"Job {jobId} is stopped. Reload the tenant to restore it."));
        }

        await scheduler.TriggerJob(jobKey, cancellationToken).ConfigureAwait(false);
        LogJobTriggered(logger, jobId, tenantId);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> StopJobAsync(string tenantId, int jobId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var jobKey = BuildJobKey(tenantId, jobId);

        var exists = await scheduler.CheckExists(jobKey, cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            return Result.Failure(Error.NotFound("SchedulerJob", jobId));
        }

        await scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        LogJobStopped(logger, jobId, tenantId);

        return Result.Success();
    }

    // ── ISchedulerAdmin — Per-Tenant Operations ──────────────────────────────

    /// <inheritdoc />
    public async Task PauseTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var groupMatcher = GroupMatcher<JobKey>.GroupEquals(BuildTenantGroup(tenantId));

        await scheduler.PauseJobs(groupMatcher, cancellationToken).ConfigureAwait(false);
        LogTenantPaused(logger, tenantId);
    }

    /// <inheritdoc />
    public async Task ResumeTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var groupMatcher = GroupMatcher<JobKey>.GroupEquals(BuildTenantGroup(tenantId));

        await scheduler.ResumeJobs(groupMatcher, cancellationToken).ConfigureAwait(false);
        LogTenantResumed(logger, tenantId);
    }

    /// <inheritdoc />
    public async Task ReloadTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var scheduler = GetScheduler();
        var group = BuildTenantGroup(tenantId);
        var groupMatcher = GroupMatcher<JobKey>.GroupEquals(group);

        // Remove all existing jobs for this tenant
        var existingJobKeys = await scheduler.GetJobKeys(groupMatcher, cancellationToken)
            .ConfigureAwait(false);

        foreach (var jobKey in existingJobKeys)
        {
            await scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        }

        LogTenantJobsRemoved(logger, tenantId, existingJobKeys.Count);

        // Re-load from database
        await LoadTenantJobsAsync(tenantId, cancellationToken).ConfigureAwait(false);

        LogTenantReloaded(logger, tenantId);
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    private async Task LoadTenantJobsAsync(string tenantId, CancellationToken cancellationToken)
    {
        if (!tenantRegistry.TryGetDatabaseName(tenantId, out var databaseName))
        {
            LogTenantNotFound(logger, tenantId);
            return;
        }

        IReadOnlyList<SchedulerConfigResponse> jobs;

        // Create a scoped service provider with tenant context set
        var scope = scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenantId, databaseName);

            var repository = scope.ServiceProvider.GetRequiredService<ISchedulerRepository>();
            jobs = await repository.GetActiveJobsAsync(cancellationToken).ConfigureAwait(false);
        }

        var scheduler = GetScheduler();
        var group = BuildTenantGroup(tenantId);

        foreach (var config in jobs)
        {
            try
            {
                var jobKey = new JobKey($"job-{config.SchJobId}", group);
                var triggerKey = new TriggerKey($"trigger-{config.SchJobId}", group);

                // Determine job type — currently all configs are EmailJob
                var jobType = "EmailJob";

                var jobDetail = JobBuilder.Create<TenantQuartzJob>()
                    .WithIdentity(jobKey)
                    .WithDescription($"{config.SchJobName} [{tenantId}]")
                    .UsingJobData(TenantQuartzJob.TenantIdKey, tenantId)
                    .UsingJobData(TenantQuartzJob.DatabaseNameKey, databaseName)
                    .UsingJobData(TenantQuartzJob.JobTypeKey, jobType)
                    .UsingJobData(TenantQuartzJob.TimeoutSecondsKey, config.SchTimeoutSeconds)
                    .UsingJobData(TenantQuartzJob.RetryAttemptsKey, config.SchRetryAttempts)
                    .UsingJobData(TenantQuartzJob.RetryIntervalSecondsKey, config.SchRetryIntervalSeconds)
                    .RequestRecovery()
                    .Build();

                // Store config object in JobDataMap (non-string, so use Put)
                jobDetail.JobDataMap.Put(TenantQuartzJob.ConfigKey, config);

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithCronSchedule(config.SchCronExpression, x => x
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .WithDescription($"Cron: {config.SchCronExpression}")
                    .Build();

                await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).ConfigureAwait(false);

                LogJobScheduled(logger, config.SchJobId, config.SchJobName, tenantId, config.SchCronExpression);
            }
            catch (Exception ex)
            {
                LogJobScheduleFailed(logger, ex, config.SchJobId, config.SchJobName, tenantId);
            }
        }
    }

    private static async Task<TenantSchedulerStatusResponse> GetTenantStatusInternalAsync(
        IScheduler scheduler, string tenantId, CancellationToken cancellationToken)
    {
        var group = BuildTenantGroup(tenantId);
        var groupMatcher = GroupMatcher<JobKey>.GroupEquals(group);
        var jobKeys = await scheduler.GetJobKeys(groupMatcher, cancellationToken).ConfigureAwait(false);
        var jobStatuses = new List<JobStatusResponse>(jobKeys.Count);

        var activeCount = 0;
        var pausedCount = 0;
        var errorCount = 0;

        foreach (var jobKey in jobKeys)
        {
            var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken).ConfigureAwait(false);
            var detail = await scheduler.GetJobDetail(jobKey, cancellationToken).ConfigureAwait(false);

            if (detail is null)
            {
                continue;
            }

            var config = detail.JobDataMap.TryGetValue(TenantQuartzJob.ConfigKey, out var configObj)
                ? (SchedulerConfigResponse)configObj
                : null;

            var triggerState = "Unknown";
            DateTimeOffset? nextFire = null;
            DateTimeOffset? prevFire = null;
            var cronExpression = string.Empty;

            if (triggers.Count > 0)
            {
                var trigger = triggers.First();
                var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken).ConfigureAwait(false);
                triggerState = state.ToString();
                nextFire = trigger.GetNextFireTimeUtc();
                prevFire = trigger.GetPreviousFireTimeUtc();

                if (trigger is ICronTrigger cronTrigger)
                {
                    cronExpression = cronTrigger.CronExpressionString ?? string.Empty;
                }
            }

            switch (triggerState)
            {
                case "Normal":
                case "Blocked":
                    activeCount++;
                    break;
                case "Paused":
                    pausedCount++;
                    break;
                case "Error":
                    errorCount++;
                    break;
            }

            jobStatuses.Add(new()
            {
                SchJobId = config?.SchJobId ?? 0,
                SchJobName = config?.SchJobName ?? jobKey.Name,
                TenantId = tenantId,
                CronExpression = cronExpression,
                State = triggerState,
                NextFireTimeUtc = nextFire,
                PreviousFireTimeUtc = prevFire
                    ?? (config?.SchLastRunDate.HasValue == true
                        ? new DateTimeOffset(config.SchLastRunDate.Value, TimeSpan.Zero)
                        : null),
                LastRunStatus = config?.SchLastRunStatus,
                LastErrorMessage = config?.SchLastErrorMessage
            });
        }

        return new()
        {
            TenantId = tenantId,
            TotalJobs = jobKeys.Count,
            ActiveJobs = activeCount,
            PausedJobs = pausedCount,
            ErrorJobs = errorCount,
            Jobs = jobStatuses
        };
    }

    private static async Task<Result<TriggerState>> GetJobTriggerStateAsync(
        IScheduler scheduler, JobKey jobKey, CancellationToken cancellationToken)
    {
        var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken).ConfigureAwait(false);

        if (triggers.Count == 0)
        {
            // Job doesn't exist or has been stopped (deleted)
            var exists = await scheduler.CheckExists(jobKey, cancellationToken).ConfigureAwait(false);

            if (!exists)
            {
                return Result.Failure<TriggerState>(Error.NotFound("SchedulerJob", jobKey.Name));
            }

            return Result.Success(TriggerState.None);
        }

        var trigger = triggers.First();
        var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken).ConfigureAwait(false);

        return Result.Success(state);
    }

    private IScheduler GetScheduler() =>
        _scheduler ?? throw new InvalidOperationException("Scheduler has not been initialized. Ensure the hosted service has started.");

    private static JobKey BuildJobKey(string tenantId, int jobId) =>
        new($"job-{jobId}", BuildTenantGroup(tenantId));

    private static string BuildTenantGroup(string tenantId) =>
        $"tenant-{tenantId}";

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Quartz scheduler starting...")]
    private static partial void LogSchedulerStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Quartz scheduler started. Loaded jobs for {TenantCount} tenant(s)")]
    private static partial void LogSchedulerStarted(ILogger logger, int tenantCount);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Quartz scheduler stopping...")]
    private static partial void LogSchedulerStopping(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Quartz scheduler stopped")]
    private static partial void LogSchedulerStopped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Scheduled job: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}', Cron='{CronExpression}'")]
    private static partial void LogJobScheduled(ILogger logger, int jobId, string jobName, string tenantId, string cronExpression);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to schedule job: JobId={JobId}, JobName='{JobName}', Tenant='{TenantId}'")]
    private static partial void LogJobScheduleFailed(ILogger logger, Exception exception, int jobId, string jobName, string tenantId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to load jobs for tenant '{TenantId}'")]
    private static partial void LogTenantLoadFailed(ILogger logger, Exception exception, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Tenant '{TenantId}' not found in registry")]
    private static partial void LogTenantNotFound(ILogger logger, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job paused: JobId={JobId}, Tenant='{TenantId}'")]
    private static partial void LogJobPaused(ILogger logger, int jobId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job resumed: JobId={JobId}, Tenant='{TenantId}'")]
    private static partial void LogJobResumed(ILogger logger, int jobId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job triggered manually: JobId={JobId}, Tenant='{TenantId}'")]
    private static partial void LogJobTriggered(ILogger logger, int jobId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Job stopped and removed: JobId={JobId}, Tenant='{TenantId}'")]
    private static partial void LogJobStopped(ILogger logger, int jobId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "All jobs paused for tenant '{TenantId}'")]
    private static partial void LogTenantPaused(ILogger logger, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "All jobs resumed for tenant '{TenantId}'")]
    private static partial void LogTenantResumed(ILogger logger, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Removed {Count} existing job(s) for tenant '{TenantId}'")]
    private static partial void LogTenantJobsRemoved(ILogger logger, string tenantId, int count);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Reloaded jobs for tenant '{TenantId}'")]
    private static partial void LogTenantReloaded(ILogger logger, string tenantId);
}
