using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Scheduler;

/// <summary>
/// Administrative interface for controlling the Quartz scheduler at runtime.
/// Supports per-job and per-tenant operations for health monitoring and management.
/// </summary>
public interface ISchedulerAdmin
{
    // ── Health & status ───────────────────────────────────────────────

    /// <summary>
    /// Returns the scheduler status for a specific tenant.
    /// </summary>
    Task<TenantSchedulerStatusResponse> GetTenantStatusAsync(string tenantId, CancellationToken cancellationToken);

    // ── Per-job operations ────────────────────────────────────────────

    /// <summary>
    /// Pauses a specific job for a tenant.
    /// </summary>
    Task PauseJobAsync(string tenantId, int jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Resumes a previously paused job for a tenant.
    /// </summary>
    Task ResumeJobAsync(string tenantId, int jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Immediately triggers a job execution. Rejects if the job is paused or stopped.
    /// </summary>
    Task<Result> TriggerJobAsync(string tenantId, int jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Stops and removes a specific job from the scheduler entirely.
    /// The job will not run until the tenant is reloaded.
    /// </summary>
    Task<Result> StopJobAsync(string tenantId, int jobId, CancellationToken cancellationToken);

    // ── Per-tenant operations ─────────────────────────────────────────

    /// <summary>
    /// Pauses all jobs for a specific tenant.
    /// </summary>
    Task PauseTenantAsync(string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Resumes all jobs for a specific tenant.
    /// </summary>
    Task ResumeTenantAsync(string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Reloads all jobs for a specific tenant from the database.
    /// Removes existing Quartz jobs for the tenant and re-schedules from current configuration.
    /// </summary>
    Task ReloadTenantAsync(string tenantId, CancellationToken cancellationToken);
}
