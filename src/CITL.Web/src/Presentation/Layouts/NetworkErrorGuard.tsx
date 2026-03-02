import { useEffect, useRef } from "react";
import { Outlet, useNavigate, useLocation } from "react-router-dom";
import { useConnectivityStore } from "@/Application/Index";

const OFFLINE_GRACE_MS = 5_000;

/**
 * Layout wrapper that monitors connectivity and redirects to /NetworkError
 * after 5 seconds of sustained offline status.
 * Initialises the ConnectivityStore on mount — all child routes inherit the connection.
 * Must wrap all routes except /NetworkError itself to avoid redirect loops.
 */
export default function NetworkErrorGuard(): React.JSX.Element
{
    const { hubStatus, networkOnline, apiReachable, init, destroy } = useConnectivityStore();
    const navigate = useNavigate();
    const location = useLocation();
    const locationRef = useRef(location);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    // Initialise connectivity monitoring (idempotent — safe across re-mounts)
    useEffect(() =>
    {
        init();
        return destroy;
    }, [ init, destroy ]);

    // Keep location ref current without triggering the main effect
    useEffect(() =>
    {
        locationRef.current = location;
    }, [ location ]);

    const isOffline = !networkOnline || (hubStatus === "disconnected" && !apiReachable);

    useEffect(() =>
    {
        if (isOffline)
        {
            timerRef.current ??= setTimeout(() =>
            {
                timerRef.current = null;
                const loc = locationRef.current;
                void navigate("/NetworkError", {
                    replace: true,
                    state: { from: loc.pathname + loc.search },
                });
            }, OFFLINE_GRACE_MS);
        }
        else
        {
            if (timerRef.current)
            {
                clearTimeout(timerRef.current);
                timerRef.current = null;
            }
        }

        return () =>
        {
            if (timerRef.current)
            {
                clearTimeout(timerRef.current);
                timerRef.current = null;
            }
        };
    }, [ isOffline, navigate ]);

    return <Outlet />;
}
