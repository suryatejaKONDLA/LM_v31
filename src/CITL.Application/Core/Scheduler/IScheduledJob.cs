namespace CITL.Application.Core.Scheduler;

/// <summary>
/// Context passed to each scheduled job execution.
/// Contains the job configuration and tenant information.
/// </summary>
public sealed class SchedulerJobContext
{
    /// <summary>
    /// The full scheduler configuration for this job.
    /// </summary>
    public required SchedulerConfigResponse Config { get; init; }

    /// <summary>
    /// The tenant identifier this job belongs to.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// The cancellation token scoped to this job execution including timeout.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// Extensible job contract. Each job type (Email, Report, etc.) implements this interface
/// and registers with a unique <see cref="JobType"/> key.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// The unique job type identifier used to resolve the correct implementation.
    /// Must match the value stored in configuration or derived from the job name.
    /// </summary>
    string JobType { get; }

    /// <summary>
    /// Executes the scheduled job logic.
    /// </summary>
    /// <param name="context">The job execution context containing configuration and tenant info.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(SchedulerJobContext context);
}
