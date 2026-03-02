using CITL.Application.Common.Hubs;
using CITL.Application.Common.Interfaces;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core;

/// <summary>
/// SignalR hub discovery and per-tenant connection metrics.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Common)]
public sealed class HubsController(
    IHubConnectionTracker tracker,
    ITenantContext tenantContext) : CitlControllerBase
{
    /// <summary>
    /// Returns all registered SignalR hubs with connection metrics for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<HubHealthResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var health = tracker.GetHealth(tenantContext.TenantId);
        return Ok(ApiResponse<IReadOnlyList<HubHealthResponse>>.Success(health));
    }

    /// <summary>
    /// Returns connection metrics for a specific hub, scoped to the current tenant.
    /// </summary>
    [HttpGet("{hubName}")]
    [ProducesResponseType(typeof(ApiResponse<HubHealthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public IActionResult Get(string hubName)
    {
        var health = tracker.GetHealth(hubName, tenantContext.TenantId);

        if (health is null)
        {
            return NotFound(ApiResponse.Error($"Hub '{hubName}' not found."));
        }

        return Ok(ApiResponse<HubHealthResponse>.Success(health));
    }
}
