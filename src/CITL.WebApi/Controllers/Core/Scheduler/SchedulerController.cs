using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Scheduler;
using CITL.SharedKernel.Results;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Scheduler;

/// <summary>
/// Monitors and controls the Quartz scheduler for the current tenant.
/// Tenant is resolved from the <c>X-Tenant-Id</c> request header.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class SchedulerController(
    ISchedulerAdmin schedulerAdmin,
    ITenantContext tenantContext) : CitlControllerBase
{
    // ── Health & Status ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns scheduler status and job details for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Tenant-level scheduler status with job details.</returns>
    /// <response code="200">Scheduler status returned.</response>
    [HttpGet("Status")]
    [ProducesResponseType(typeof(ApiResponse<TenantSchedulerStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        var status = await schedulerAdmin.GetTenantStatusAsync(tenantContext.TenantId, cancellationToken);
        return FromResult(
            Result.Success(status),
            "Scheduler status retrieved.");
    }

    // ── Per-Job Operations ───────────────────────────────────────────────────

    /// <summary>
    /// Pauses a specific job.
    /// </summary>
    /// <param name="jobId">The scheduler job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Job paused successfully.</response>
    [HttpPost("Job/{jobId:int}/Pause")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PauseJobAsync(int jobId, CancellationToken cancellationToken)
    {
        await schedulerAdmin.PauseJobAsync(tenantContext.TenantId, jobId, cancellationToken);
        return FromResult(Result.Success(), $"Job {jobId} paused.");
    }

    /// <summary>
    /// Resumes a previously paused job.
    /// </summary>
    /// <param name="jobId">The scheduler job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Job resumed successfully.</response>
    [HttpPost("Job/{jobId:int}/Resume")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumeJobAsync(int jobId, CancellationToken cancellationToken)
    {
        await schedulerAdmin.ResumeJobAsync(tenantContext.TenantId, jobId, cancellationToken);
        return FromResult(Result.Success(), $"Job {jobId} resumed.");
    }

    /// <summary>
    /// Immediately triggers a job execution. Rejects if the job is paused or stopped.
    /// </summary>
    /// <param name="jobId">The scheduler job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Job triggered successfully.</response>
    /// <response code="400">Job is paused or stopped.</response>
    [HttpPost("Job/{jobId:int}/Trigger")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriggerJobAsync(int jobId, CancellationToken cancellationToken)
    {
        var result = await schedulerAdmin.TriggerJobAsync(tenantContext.TenantId, jobId, cancellationToken);
        return FromResult(result, $"Job {jobId} triggered.");
    }

    /// <summary>
    /// Stops and removes a job from the scheduler entirely.
    /// The job will not run until the tenant is reloaded.
    /// </summary>
    /// <param name="jobId">The scheduler job identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Job stopped successfully.</response>
    /// <response code="404">Job not found.</response>
    [HttpPost("Job/{jobId:int}/Stop")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopJobAsync(int jobId, CancellationToken cancellationToken)
    {
        var result = await schedulerAdmin.StopJobAsync(tenantContext.TenantId, jobId, cancellationToken);
        return FromResult(result, $"Job {jobId} stopped.");
    }

    // ── Tenant-Level Operations ──────────────────────────────────────────────

    /// <summary>
    /// Pauses all jobs for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">All jobs paused.</response>
    [HttpPost("PauseAll")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PauseAllAsync(CancellationToken cancellationToken)
    {
        await schedulerAdmin.PauseTenantAsync(tenantContext.TenantId, cancellationToken);
        return FromResult(Result.Success(), "All jobs paused.");
    }

    /// <summary>
    /// Resumes all jobs for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">All jobs resumed.</response>
    [HttpPost("ResumeAll")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumeAllAsync(CancellationToken cancellationToken)
    {
        await schedulerAdmin.ResumeTenantAsync(tenantContext.TenantId, cancellationToken);
        return FromResult(Result.Success(), "All jobs resumed.");
    }

    /// <summary>
    /// Reloads all jobs for the current tenant from the database.
    /// Use after modifying <c>Scheduler_Configuration</c> table rows.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Jobs reloaded.</response>
    [HttpPost("Reload")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReloadAsync(CancellationToken cancellationToken)
    {
        await schedulerAdmin.ReloadTenantAsync(tenantContext.TenantId, cancellationToken);
        return FromResult(Result.Success(), "Jobs reloaded.");
    }
}
