using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks Quartz scheduler health — verifies the scheduler is started and not in standby.
/// Reports metadata about running triggers and job groups.
/// </summary>
internal sealed class QuartzSchedulerHealthCheck(ISchedulerFactory schedulerFactory) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        try
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);

            data["SchedulerName"] = scheduler.SchedulerName;
            data["IsStarted"] = scheduler.IsStarted;
            data["InStandbyMode"] = scheduler.InStandbyMode;
            data["IsShutdown"] = scheduler.IsShutdown;

            var metadata = await scheduler.GetMetaData(cancellationToken).ConfigureAwait(false);
            data["RunningSince"] = metadata.RunningSince?.UtcDateTime.ToString("o") ?? "N/A";
            data["NumberOfJobsExecuted"] = metadata.NumberOfJobsExecuted;

            var jobGroups = await scheduler.GetJobGroupNames(cancellationToken).ConfigureAwait(false);
            data["JobGroups"] = jobGroups.Count;

            if (scheduler.IsShutdown)
            {
                return HealthCheckResult.Unhealthy("Quartz scheduler has been shut down.", data: data);
            }

            if (scheduler.InStandbyMode)
            {
                return HealthCheckResult.Degraded("Quartz scheduler is in standby mode.", data: data);
            }

            if (!scheduler.IsStarted)
            {
                return HealthCheckResult.Degraded("Quartz scheduler has not started yet.", data: data);
            }

            return HealthCheckResult.Healthy("Quartz scheduler is running.", data);
        }
        catch (Exception ex)
        {
            data["Error"] = ex.Message;
            return HealthCheckResult.Unhealthy("Unable to check Quartz scheduler.", ex, data);
        }
    }
}
