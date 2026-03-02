import { create } from "zustand";

/** Geolocation permission / fetch status. */
export type LocationStatus = "idle" | "granted" | "denied" | "unavailable";

interface LocationState
{
    readonly status: LocationStatus;

    /** Last successfully retrieved position. */
    readonly position: GeolocationPosition | null;

    /** `true` when the user has explicitly denied the permission. */
    readonly isDenied: boolean;

    setGranted: (position: GeolocationPosition) => void;
    setDenied: () => void;
    setUnavailable: () => void;

    /** Reset to `idle` — used by LocationBlockedModal before a retry attempt. */
    resetToIdle: () => void;
}

export const useLocationStore = create<LocationState>((set) => ({
    status: "idle",
    position: null,
    isDenied: false,

    setGranted: (position) =>
    {
        set({ status: "granted", position, isDenied: false });
    },
    setDenied: () =>
    {
        set({ status: "denied", isDenied: true });
    },
    setUnavailable: () =>
    {
        set({ status: "unavailable", isDenied: false });
    },
    resetToIdle: () =>
    {
        set({ status: "idle", isDenied: false });
    },
}));
