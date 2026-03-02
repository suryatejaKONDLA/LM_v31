using System.Collections.Concurrent;
using CITL.Application.Common.Hubs;

namespace CITL.WebApi.Hubs;

/// <summary>
/// Thread-safe singleton tracker for SignalR hub connections.
/// </summary>
internal sealed class HubConnectionTracker : IHubConnectionTracker
{
    private readonly ConcurrentDictionary<string, HubState> _hubs = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void RegisterHub(HubDescriptor descriptor)
    {
        _hubs.GetOrAdd(descriptor.Name, _ => new HubState(descriptor));
    }

    /// <inheritdoc />
    public void OnConnected(string hubName, string connectionId, bool isAuthenticated, string? tenantId, string? userId)
    {
        if (!_hubs.TryGetValue(hubName, out var state))
        {
            return;
        }

        state.Connections.TryAdd(connectionId, new ConnectionInfo
        {
            IsAuthenticated = isAuthenticated,
            TenantId = tenantId,
            UserId = userId
        });
    }

    /// <inheritdoc />
    public void OnDisconnected(string hubName, string connectionId)
    {
        if (!_hubs.TryGetValue(hubName, out var state))
        {
            return;
        }

        state.Connections.TryRemove(connectionId, out _);
    }

    /// <inheritdoc />
    public IReadOnlyList<HubHealthResponse> GetHealth(string tenantId)
    {
        return _hubs.Values
            .Select(s => BuildTenantHealth(s, tenantId))
            .OrderBy(h => h.Hub.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public HubHealthResponse? GetHealth(string hubName, string tenantId)
    {
        return _hubs.TryGetValue(hubName, out var state)
            ? BuildTenantHealth(state, tenantId)
            : null;
    }

    /// <inheritdoc />
    public SignalRHealthSummary GetOverallHealth()
    {
        var hubSummaries = _hubs.Values
            .Select(s =>
            {
                var connections = s.Connections.Values.ToList();
                return new HubConnectionSummary
                {
                    Name = s.Descriptor.Name,
                    Connections = connections.Count,
                    Tenants = connections
                        .Where(c => c.TenantId is not null)
                        .Select(c => c.TenantId!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count()
                };
            })
            .OrderBy(h => h.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new SignalRHealthSummary
        {
            RegisteredHubs = hubSummaries.Count,
            TotalConnections = hubSummaries.Sum(h => h.Connections),
            Hubs = hubSummaries
        };
    }

    private static HubHealthResponse BuildTenantHealth(HubState state, string tenantId)
    {
        var tenantConnections = state.Connections.Values
            .Count(c => string.Equals(c.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

        return new HubHealthResponse
        {
            Hub = state.Descriptor,
            TotalConnections = tenantConnections,
            RegisteredAtUtc = state.RegisteredAtUtc,
            IsHealthy = true
        };
    }

    private sealed class HubState(HubDescriptor descriptor)
    {
        public HubDescriptor Descriptor { get; } = descriptor;
        public ConcurrentDictionary<string, ConnectionInfo> Connections { get; } = new();
        public DateTime RegisteredAtUtc { get; } = DateTime.UtcNow;
    }

    private sealed class ConnectionInfo
    {
        public required bool IsAuthenticated { get; init; }
        public required string? TenantId { get; init; }
        public required string? UserId { get; init; }
    }
}
