namespace CITL.Application.Core.Scheduler;

/// <summary>
/// Repository interface for reading scheduler configuration from <c>citl_sys.Scheduler_Configuration</c>.
/// </summary>
public interface ISchedulerRepository
{
    /// <summary>
    /// Retrieves all active scheduler configuration rows.
    /// </summary>
    Task<IReadOnlyList<SchedulerConfigResponse>> GetActiveJobsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the last run tracking columns for a specific job.
    /// </summary>
    /// <param name="jobId">The scheduler job identifier.</param>
    /// <param name="lastRunDate">The UTC date/time when the job last ran.</param>
    /// <param name="nextRunDate">The calculated next fire time.</param>
    /// <param name="status">The last run status (e.g., "Success", "Failed").</param>
    /// <param name="errorMessage">The error message if the job failed, otherwise <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateLastRunAsync(
        int jobId,
        DateTime lastRunDate,
        DateTime? nextRunDate,
        string status,
        string? errorMessage,
        CancellationToken cancellationToken);
}
