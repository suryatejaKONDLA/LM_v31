import { create } from "zustand";
import { type ConnectionStatus, PingHubConnection } from "@/Infrastructure/Index";

interface ConnectivityState
{
    /** PingHub SignalR connection status. */
    hubStatus: ConnectionStatus;

    /** Browser navigator.onLine status. */
    networkOnline: boolean;

    /** True when the last API call succeeded (no network error). */
    apiReachable: boolean;

    /** True after init() has been called at least once. */
    initialized: boolean;

    /** Start PingHub connection and attach all event listeners. Idempotent — safe to call multiple times. */
    init: () => void;

    /** Tear down connection and listeners. */
    destroy: () => void;
}

export const useConnectivityStore = create<ConnectivityState>((set, get) =>
{
    let unsubHub: (() => void) | null = null;
    let onlineHandler: (() => void) | null = null;
    let offlineHandler: (() => void) | null = null;
    let apiErrorHandler: (() => void) | null = null;
    let apiRecoveryTimer: ReturnType<typeof setTimeout> | null = null;

    return {
        hubStatus: "disconnected",
        networkOnline: typeof navigator !== "undefined" ? navigator.onLine : true,
        apiReachable: true,
        initialized: false,

        init()
        {
            // Idempotent — skip if already initialized
            if (get().initialized)
            {
                return;
            }

            set({ initialized: true });
            // Subscribe to PingHub status changes
            unsubHub = PingHubConnection.subscribe((status) =>
            {
                set({ hubStatus: status });

                // When hub reconnects, API is reachable again
                if (status === "connected")
                {
                    set({ apiReachable: true });
                }

                // When hub fully disconnects (all retries exhausted), API is unreachable
                if (status === "disconnected")
                {
                    set({ apiReachable: false });
                }
            });

            // Browser online/offline events
            onlineHandler = () =>
            {
                set({ networkOnline: true });
            };
            offlineHandler = () =>
            {
                set({ networkOnline: false });
            };
            window.addEventListener("online", onlineHandler);
            window.addEventListener("offline", offlineHandler);

            // ApiClient fires this custom event on network failures
            apiErrorHandler = () =>
            {
                set({ apiReachable: false });

                // Auto-recover after 10s if no further errors
                if (apiRecoveryTimer)
                {
                    clearTimeout(apiRecoveryTimer);
                }

                apiRecoveryTimer = setTimeout(() =>
                {
                    if (get().hubStatus === "connected")
                    {
                        set({ apiReachable: true });
                    }
                }, 10_000);
            };

            window.addEventListener("api-network-error", apiErrorHandler);

            // Start the hub connection (deferred to avoid blocking render)
            if (typeof requestIdleCallback !== "undefined")
            {
                requestIdleCallback(() => void PingHubConnection.start());
            }
            else
            {
                setTimeout(() => void PingHubConnection.start(), 100);
            }
        },

        destroy()
        {
            set({ initialized: false });

            unsubHub?.();
            unsubHub = null;

            if (onlineHandler)
            {
                window.removeEventListener("online", onlineHandler);
            }

            if (offlineHandler)
            {
                window.removeEventListener("offline", offlineHandler);
            }

            if (apiErrorHandler)
            {
                window.removeEventListener("api-network-error", apiErrorHandler);
            }

            if (apiRecoveryTimer)
            {
                clearTimeout(apiRecoveryTimer);
                apiRecoveryTimer = null;
            }

            void PingHubConnection.stop();
        },
    };
});
