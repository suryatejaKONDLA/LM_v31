using CITL.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CITL.WebApi.Hubs;

/// <summary>
/// SignalR-backed implementation of <see cref="INotificationSender"/>.
/// Uses <see cref="IHubContext{THub}"/> so any service can push notifications
/// without coupling to the hub directly.
/// </summary>
/// <param name="hubContext">The SignalR hub context for NotificationHub.</param>
public sealed class SignalRNotificationSender(
    IHubContext<NotificationHub> hubContext) : INotificationSender
{
    /// <inheritdoc />
    public Task SendToTenantAsync(
        string tenantId,
        string method,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var group = NotificationHub.FormatTenantGroup(tenantId);
        return hubContext.Clients.Group(group).SendAsync(method, payload, cancellationToken);
    }

    /// <inheritdoc />
    public Task SendToUserAsync(
        string userId,
        string method,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.User(userId).SendAsync(method, payload, cancellationToken);
    }

    /// <inheritdoc />
    public Task SendToAllAsync(
        string method,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.All.SendAsync(method, payload, cancellationToken);
    }
}
