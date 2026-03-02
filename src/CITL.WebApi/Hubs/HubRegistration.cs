using CITL.Application.Common.Hubs;

namespace CITL.WebApi.Hubs;

/// <summary>
/// Central registry of all SignalR hub descriptors.
/// Add new hubs here — they will automatically appear in the discovery endpoint and health checks.
/// </summary>
internal static class HubRegistration
{
    /// <summary>
    /// Registers all hub descriptors into the tracker at startup.
    /// </summary>
    internal static void SeedAll(IHubConnectionTracker tracker)
    {
        tracker.RegisterHub(new HubDescriptor
        {
            Name = nameof(PingHub),
            Path = "/Hubs/Ping",
            RequiresAuthentication = false,
            RequiresTenant = false,
            Description = "Connectivity check — connect on app startup, stays alive until browser tab closes.",
            ServerMethods =
            [
                new HubMethodDescriptor
                {
                    Name = nameof(PingHub.Ping),
                    Returns = "string",
                    Description = "Returns \"pong\" to verify round-trip connectivity."
                }
            ],
            ClientEvents = []
        });

        tracker.RegisterHub(new HubDescriptor
        {
            Name = nameof(NotificationHub),
            Path = "/Hubs/Notifications",
            RequiresAuthentication = true,
            RequiresTenant = true,
            Description = "Real-time notifications — tenant group auto-joined on connect.",
            ServerMethods = [],
            ClientEvents =
            [
                new HubMethodDescriptor
                {
                    Name = "ReceiveNotification",
                    Returns = "object",
                    Description = "Receives a notification pushed by the server."
                }
            ]
        });
    }
}
