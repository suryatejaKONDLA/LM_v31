import { HubConnectionBuilder, HubConnectionState, type HubConnection, LogLevel } from "@microsoft/signalr";
import { AppConfig } from "@/Shared/Index";

export type ConnectionStatus = "connected" | "reconnecting" | "disconnected";

type StatusListener = (status: ConnectionStatus) => void;

const RECONNECT_DELAYS = [ 0, 2_000, 5_000, 10_000 ];

/**
 * Singleton manager for the PingHub SignalR connection.
 * Anonymous hub — no auth token required.
 */
export const PingHubConnection = (() =>
{
    let connection: HubConnection | null = null;
    const listeners = new Set<StatusListener>();

    function notify(status: ConnectionStatus): void
    {
        for (const listener of listeners)
        {
            listener(status);
        }
    }

    function buildConnection(): HubConnection
    {
        const hubUrl = `${AppConfig.ApiBaseUrl}Hubs/Ping`;

        return new HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect(RECONNECT_DELAYS)
            .configureLogging(AppConfig.IsDevelopment ? LogLevel.Information : LogLevel.Warning)
            .build();
    }

    function attachEvents(conn: HubConnection): void
    {
        conn.onreconnecting(() =>
        {
            notify("reconnecting");
        });
        conn.onreconnected(() =>
        {
            notify("connected");
        });
        conn.onclose(() =>
        {
            notify("disconnected");
        });
    }

    async function start(): Promise<void>
    {
        if (connection?.state === HubConnectionState.Connected)
        {
            return;
        }

        if (!connection)
        {
            connection = buildConnection();
            attachEvents(connection);
        }

        try
        {
            await connection.start();
            notify("connected");
        }
        catch
        {
            notify("disconnected");
        }
    }

    async function stop(): Promise<void>
    {
        if (connection)
        {
            await connection.stop();
            connection = null;
            notify("disconnected");
        }
    }

    async function restart(): Promise<void>
    {
        await stop();
        await start();
    }

    function subscribe(listener: StatusListener): () => void
    {
        listeners.add(listener);
        return () => listeners.delete(listener);
    }

    function getStatus(): ConnectionStatus
    {
        if (!connection)
        {
            return "disconnected";
        }

        switch (connection.state)
        {
            case HubConnectionState.Connected:
                return "connected";
            case HubConnectionState.Reconnecting:
            case HubConnectionState.Connecting:
                return "reconnecting";
            default:
                return "disconnected";
        }
    }

    return { start, stop, restart, subscribe, getStatus } as const;
})();
