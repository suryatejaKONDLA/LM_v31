namespace CITL.Application.Common.Hubs;

/// <summary>
/// Tracks real-time SignalR hub connections.
/// </summary>
public interface IHubConnectionTracker
{
    /// <summary>
    /// Registers a hub descriptor so it appears in discovery even with zero connections.
    /// </summary>
    void RegisterHub(HubDescriptor descriptor);

    /// <summary>
    /// Records a client connection to a hub.
    /// </summary>
    void OnConnected(string hubName, string connectionId, bool isAuthenticated, string? tenantId, string? userId);

    /// <summary>
    /// Records a client disconnection from a hub.
    /// </summary>
    void OnDisconnected(string hubName, string connectionId);

    /// <summary>
    /// Returns health and connection metrics for all hubs, filtered to a specific tenant.
    /// </summary>
    IReadOnlyList<HubHealthResponse> GetHealth(string tenantId);

    /// <summary>
    /// Returns health and connection metrics for a specific hub, filtered to a specific tenant.
    /// </summary>
    HubHealthResponse? GetHealth(string hubName, string tenantId);

    /// <summary>
    /// Returns aggregate health across all hubs (unfiltered). Used by infrastructure health checks.
    /// </summary>
    SignalRHealthSummary GetOverallHealth();
}
