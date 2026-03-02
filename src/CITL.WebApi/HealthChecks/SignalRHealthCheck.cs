using CITL.Application.Common.Hubs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CITL.WebApi.HealthChecks;

/// <summary>
/// Reports SignalR hub registration status and aggregate connection metrics.
/// </summary>
internal sealed class SignalRHealthCheck(IHubConnectionTracker tracker) : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var summary = tracker.GetOverallHealth();

        var data = new Dictionary<string, object>
        {
            ["RegisteredHubs"] = summary.RegisteredHubs,
            ["TotalConnections"] = summary.TotalConnections
        };

        foreach (var hub in summary.Hubs)
        {
            data[$"Hub:{hub.Name}:Connections"] = hub.Connections;
            data[$"Hub:{hub.Name}:Tenants"] = hub.Tenants;
        }

        var result = summary.RegisteredHubs > 0
            ? HealthCheckResult.Healthy("SignalR hubs are registered and operational.", data)
            : HealthCheckResult.Degraded("No SignalR hubs registered.", data: data);

        return Task.FromResult(result);
    }
}
