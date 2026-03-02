import { useEffect } from "react";
import { useLocationStore } from "./Stores/Index";

const GeoOptions: PositionOptions = {
    enableHighAccuracy: true,
    timeout: 10_000,
    maximumAge: 300_000,
};

/**
 * Initialises browser geolocation and watches for permission changes.
 * Call once at the application root — drives the global `LocationStore`.
 *
 * Strategy (avoids the Chrome "user gesture" violation):
 * - `granted`  → request position immediately (no prompt = no violation).
 * - `denied`   → mark store as denied immediately without calling the API.
 * - `prompt`   → defer the request until the first user interaction
 *               (click or keydown), after which the browser shows its prompt
 *               as a direct result of a user gesture.
 *
 * Additionally subscribes to the Permissions API `onchange` event so that
 * toggling the browser permission after page load updates the store instantly.
 */
export function useLocationPermission(): void
{
    const { setGranted, setDenied } = useLocationStore();

    useEffect(() =>
    {
        const requestPosition = () =>
        {
            navigator.geolocation.getCurrentPosition(
                (pos) =>
                {
                    setGranted(pos);
                },
                (err) =>
                {
                    if (err.code === GeolocationPositionError.PERMISSION_DENIED)
                    {
                        setDenied();
                    }
                    // TIMEOUT / POSITION_UNAVAILABLE — leave status as-is
                },
                GeoOptions,
            );
        };

        let permissionStatus: PermissionStatus | null = null;

        // Clean up deferred user-gesture listener if still pending
        let removeGestureListener: (() => void) | null = null;

        void navigator.permissions
            .query({ name: "geolocation" })
            .then((ps) =>
            {
                permissionStatus = ps;

                if (ps.state === "granted")
                {
                    // Already allowed — safe to call immediately, no browser prompt
                    requestPosition();
                }
                else if (ps.state === "denied")
                {
                    // Already blocked — no point calling getCurrentPosition
                    setDenied();
                }
                else
                {
                    // "prompt" — defer until the first user gesture to avoid the
                    // Chrome DevTools violation: "Only request geolocation in
                    // response to a user gesture."
                    const onGesture = () =>
                    {
                        requestPosition();
                        removeGestureListener?.();
                    };

                    document.addEventListener("click", onGesture, { once: true });
                    document.addEventListener("keydown", onGesture, { once: true });

                    removeGestureListener = () =>
                    {
                        document.removeEventListener("click", onGesture);
                        document.removeEventListener("keydown", onGesture);
                    };
                }

                // Watch for runtime permission changes (e.g. user toggles in address bar)
                ps.onchange = () =>
                {
                    if (ps.state === "denied")
                    {
                        setDenied();
                    }
                    else if (ps.state === "granted")
                    {
                        requestPosition();
                    }
                };
            })
            .catch(() =>
            {
                // Permissions API unavailable (Firefox private mode, etc.)
                // Fall back to requesting directly — violation may appear
                // in older browsers but the feature still works.
                requestPosition();
            });

        return () =>
        {
            removeGestureListener?.();
            if (permissionStatus)
            {
                permissionStatus.onchange = null;
            }
        };
    }, [ setGranted, setDenied ]);
}
