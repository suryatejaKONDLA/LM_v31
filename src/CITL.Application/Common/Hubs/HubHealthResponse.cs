namespace CITL.Application.Common.Hubs;

/// <summary>
/// Connection metrics for a single SignalR hub, scoped to a tenant.
/// </summary>
public sealed class HubHealthResponse
{
    /// <summary>Hub metadata.</summary>
    public required HubDescriptor Hub { get; init; }

    /// <summary>Active connections for the requesting tenant.</summary>
    public required int TotalConnections { get; init; }

    /// <summary>UTC timestamp when the hub was registered.</summary>
    public required DateTime RegisteredAtUtc { get; init; }

    /// <summary>Whether the hub is accepting connections.</summary>
    public required bool IsHealthy { get; init; }
}

/// <summary>
/// Aggregate SignalR health summary across all hubs for infrastructure health checks.
/// </summary>
public sealed class SignalRHealthSummary
{
    /// <summary>Number of registered hubs.</summary>
    public required int RegisteredHubs { get; init; }

    /// <summary>Total connections across all hubs and tenants.</summary>
    public required int TotalConnections { get; init; }

    /// <summary>Per-hub connection breakdown.</summary>
    public required IReadOnlyList<HubConnectionSummary> Hubs { get; init; }
}

/// <summary>
/// Connection count summary for a single hub.
/// </summary>
public sealed class HubConnectionSummary
{
    /// <summary>Hub name.</summary>
    public required string Name { get; init; }

    /// <summary>Active connections on this hub.</summary>
    public required int Connections { get; init; }

    /// <summary>Number of distinct tenants connected.</summary>
    public required int Tenants { get; init; }
}
