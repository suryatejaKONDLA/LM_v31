namespace CITL.Application.Common.Hubs;

/// <summary>
/// Metadata describing a registered SignalR hub.
/// </summary>
public sealed class HubDescriptor
{
    /// <summary>Hub name (e.g. "PingHub").</summary>
    public required string Name { get; init; }

    /// <summary>WebSocket URL path (e.g. "/Hubs/Ping").</summary>
    public required string Path { get; init; }

    /// <summary>Whether the hub requires a valid JWT access token.</summary>
    public required bool RequiresAuthentication { get; init; }

    /// <summary>Whether the hub requires multi-tenant context.</summary>
    public required bool RequiresTenant { get; init; }

    /// <summary>Human-readable description.</summary>
    public required string Description { get; init; }

    /// <summary>Methods the client can invoke on the server.</summary>
    public required IReadOnlyList<HubMethodDescriptor> ServerMethods { get; init; }

    /// <summary>Events the server can push to the client.</summary>
    public required IReadOnlyList<HubMethodDescriptor> ClientEvents { get; init; }
}

/// <summary>
/// Describes a single server method or client event on a hub.
/// </summary>
public sealed class HubMethodDescriptor
{
    /// <summary>Method or event name.</summary>
    public required string Name { get; init; }

    /// <summary>Return type description.</summary>
    public required string Returns { get; init; }

    /// <summary>Human-readable description.</summary>
    public required string Description { get; init; }
}
