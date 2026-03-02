using CITL.Application.Common.Hubs;
using CITL.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CITL.WebApi.Hubs;

/// <summary>
/// Anonymous hub for connectivity checks.
/// Clients connect on app startup and stay connected until the browser tab closes.
/// </summary>
[AllowAnonymous]
[BypassTenant]
public sealed class PingHub(IHubConnectionTracker tracker) : Hub
{
    /// <summary>
    /// Returns a pong response to verify round-trip connectivity.
    /// </summary>
    public static string Ping() => "Pong";

    /// <inheritdoc />
    public override Task OnConnectedAsync()
    {
        tracker.OnConnected(nameof(PingHub), Context.ConnectionId, isAuthenticated: false, tenantId: null, userId: null);
        return base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        tracker.OnDisconnected(nameof(PingHub), Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
