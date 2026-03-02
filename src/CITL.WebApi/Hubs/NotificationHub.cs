using System.Security.Claims;
using CITL.Application.Common.Hubs;
using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CITL.WebApi.Hubs;

/// <summary>
/// Authenticated hub for real-time notifications.
/// Clients connect after login with JWT access token.
/// Each user auto-joins their tenant group for tenant-scoped broadcasts.
/// </summary>
[Authorize]
public sealed partial class NotificationHub(
    ITenantRegistry tenantRegistry,
    IHubConnectionTracker tracker,
    ILogger<NotificationHub> logger) : Hub
{
    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirstValue(TenantConstants.JwtClaimType);
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(tenantId) && tenantRegistry.TryGetDatabaseName(tenantId, out _))
        {
            var tenantGroup = FormatTenantGroup(tenantId);
            await Groups.AddToGroupAsync(Context.ConnectionId, tenantGroup);
            LogClientConnected(logger, Context.ConnectionId, tenantId);
        }

        tracker.OnConnected(nameof(NotificationHub), Context.ConnectionId, isAuthenticated: true, tenantId, userId);

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirstValue(TenantConstants.JwtClaimType);

        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantGroup = FormatTenantGroup(tenantId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantGroup);
            LogClientDisconnected(logger, Context.ConnectionId, tenantId);
        }

        tracker.OnDisconnected(nameof(NotificationHub), Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Formats the SignalR group name for a tenant.
    /// </summary>
    internal static string FormatTenantGroup(string tenantId) => $"tenant:{tenantId}";

    [LoggerMessage(Level = LogLevel.Information,
        Message = "SignalR client connected: {ConnectionId}, tenant: {TenantId}")]
    private static partial void LogClientConnected(
        ILogger logger, string connectionId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "SignalR client disconnected: {ConnectionId}, tenant: {TenantId}")]
    private static partial void LogClientDisconnected(
        ILogger logger, string connectionId, string tenantId);
}
