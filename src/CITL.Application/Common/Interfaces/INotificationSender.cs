namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Abstraction for pushing real-time notifications to connected clients.
/// Implemented in the WebApi layer via SignalR.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Sends a notification to all connected clients in the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="method">The client method name to invoke.</param>
    /// <param name="payload">The payload to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToTenantAsync(string tenantId, string method, object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to a specific user by their user identifier claim.
    /// </summary>
    /// <param name="userId">The user identifier (maps to ClaimTypes.NameIdentifier).</param>
    /// <param name="method">The client method name to invoke.</param>
    /// <param name="payload">The payload to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToUserAsync(string userId, string method, object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all connected authenticated clients.
    /// </summary>
    /// <param name="method">The client method name to invoke.</param>
    /// <param name="payload">The payload to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToAllAsync(string method, object payload, CancellationToken cancellationToken = default);
}
