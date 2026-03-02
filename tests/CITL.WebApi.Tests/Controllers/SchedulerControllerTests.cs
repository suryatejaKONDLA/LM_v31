using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Scheduler;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Scheduler;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="SchedulerController"/>.
/// Verifies correct delegation to <see cref="ISchedulerAdmin"/> and HTTP response mapping.
/// </summary>
public sealed class SchedulerControllerTests
{
    private readonly ISchedulerAdmin _schedulerAdmin = Substitute.For<ISchedulerAdmin>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly SchedulerController _controller;

    public SchedulerControllerTests()
    {
        _tenantContext.TenantId.Returns("T1");
        _controller = new(_schedulerAdmin, _tenantContext);
    }

    // ── GetStatusAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_Returns200WithStatus()
    {
        // Arrange
        var status = new TenantSchedulerStatusResponse
        {
            TenantId = "T1",
            TotalJobs = 3,
            ActiveJobs = 2,
            PausedJobs = 1,
            ErrorJobs = 0,
            Jobs = []
        };
        _schedulerAdmin.GetTenantStatusAsync("T1", Arg.Any<CancellationToken>())
            .Returns(status);

        // Act
        var actionResult = await _controller.GetStatusAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<TenantSchedulerStatusResponse>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Equal(3, apiResponse.Data!.TotalJobs);
    }

    [Fact]
    public async Task GetStatusAsync_PassesTenantIdFromContext()
    {
        // Arrange
        _schedulerAdmin.GetTenantStatusAsync("T1", Arg.Any<CancellationToken>())
            .Returns(new TenantSchedulerStatusResponse { TenantId = "T1" });

        // Act
        await _controller.GetStatusAsync(CancellationToken.None);

        // Assert
        await _schedulerAdmin.Received(1)
            .GetTenantStatusAsync("T1", Arg.Any<CancellationToken>());
    }

    // ── PauseJobAsync ────────────────────────────────────────────────

    [Fact]
    public async Task PauseJobAsync_Returns200()
    {
        // Act
        var actionResult = await _controller.PauseJobAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse>(okResult.Value);
        await _schedulerAdmin.Received(1)
            .PauseJobAsync("T1", 1, Arg.Any<CancellationToken>());
    }

    // ── ResumeJobAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ResumeJobAsync_Returns200()
    {
        // Act
        var actionResult = await _controller.ResumeJobAsync(2, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        await _schedulerAdmin.Received(1)
            .ResumeJobAsync("T1", 2, Arg.Any<CancellationToken>());
    }

    // ── TriggerJobAsync ──────────────────────────────────────────────

    [Fact]
    public async Task TriggerJobAsync_WhenSuccess_Returns200()
    {
        // Arrange
        _schedulerAdmin.TriggerJobAsync("T1", 1, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.TriggerJobAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task TriggerJobAsync_WhenJobPaused_Returns400()
    {
        // Arrange
        _schedulerAdmin.TriggerJobAsync("T1", 1, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(new Error("Scheduler.JobPaused", "Job is paused.")));

        // Act
        var actionResult = await _controller.TriggerJobAsync(1, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── StopJobAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task StopJobAsync_WhenSuccess_Returns200()
    {
        // Arrange
        _schedulerAdmin.StopJobAsync("T1", 1, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var actionResult = await _controller.StopJobAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
    }

    [Fact]
    public async Task StopJobAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _schedulerAdmin.StopJobAsync("T1", 99, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("Scheduler.Job", "99")));

        // Act
        var actionResult = await _controller.StopJobAsync(99, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── PauseAllAsync ────────────────────────────────────────────────

    [Fact]
    public async Task PauseAllAsync_Returns200()
    {
        // Act
        var actionResult = await _controller.PauseAllAsync(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        await _schedulerAdmin.Received(1)
            .PauseTenantAsync("T1", Arg.Any<CancellationToken>());
    }

    // ── ResumeAllAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ResumeAllAsync_Returns200()
    {
        // Act
        var actionResult = await _controller.ResumeAllAsync(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        await _schedulerAdmin.Received(1)
            .ResumeTenantAsync("T1", Arg.Any<CancellationToken>());
    }

    // ── ReloadAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ReloadAsync_Returns200()
    {
        // Act
        var actionResult = await _controller.ReloadAsync(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        await _schedulerAdmin.Received(1)
            .ReloadTenantAsync("T1", Arg.Any<CancellationToken>());
    }
}
