import { AuthStorage } from "./AuthStorage";
import { ApiResponseCode, type ApiResponseWithData, AppConfig, MediaTypeConstants, TenantConstants } from "@/Shared/Index";

/** Auth controller route — inlined to avoid circular dependency with Persistence barrel. */
const AuthRoute = "Auth";

/** Buffer in milliseconds — refresh 2 minutes before expiry. */
const REFRESH_BUFFER_MS = 2 * 60 * 1000;

/** Minimum delay to avoid tight loops (10 seconds). */
const MIN_DELAY_MS = 10_000;

interface RefreshResponse
{
    Access_Token: string;
    Refresh_Token: string;
}

// ── Module-level mutable state ────────────────────────────────────
let timerId: ReturnType<typeof setTimeout> | null = null;
let listening = false;
let refreshing = false;

function scheduleRefresh(): void
{
    const timeUntilExpiry = AuthStorage.getTimeUntilExpiryMs();

    if (timeUntilExpiry <= 0)
    {
        return;
    }

    const delay = Math.max(timeUntilExpiry - REFRESH_BUFFER_MS, MIN_DELAY_MS);

    timerId = setTimeout(() =>
    {
        void doRefresh();
    }, delay);
}

async function doRefresh(): Promise<boolean>
{
    if (refreshing)
    {
        return false;
    }

    const refreshToken = AuthStorage.getRefreshToken();
    const loginUser = AuthStorage.getLoginUser();

    if (!refreshToken || !loginUser)
    {
        return false;
    }

    refreshing = true;

    try
    {
        const response = await fetch(
            `${AppConfig.ApiBaseUrl}${AuthRoute}/Refresh`,
            {
                method: "POST",
                headers: {
                    "Content-Type": MediaTypeConstants.Json,
                    [ TenantConstants.HeaderName ]: AppConfig.TenantId,
                },
                body: JSON.stringify({
                    Refresh_Token: refreshToken,
                    Login_User: loginUser,
                }),
            },
        );

        if (!response.ok)
        {
            return false;
        }

        const data = await response.json() as ApiResponseWithData<RefreshResponse>;

        if (data.Code === ApiResponseCode.Success)
        {
            AuthStorage.setTokens(data.Data.Access_Token, data.Data.Refresh_Token);
            scheduleRefresh();
            return true;
        }

        return false;
    }
    catch
    {
        return false;
    }
    finally
    {
        refreshing = false;
    }
}

function onVisibilityChange(): void
{
    if (document.visibilityState !== "visible")
    {
        return;
    }

    if (!AuthStorage.isAuthenticated())
    {
        return;
    }

    if (AuthStorage.isTokenNearExpiry())
    {
        void doRefresh();
    }
    else
    {
        // Reschedule — timer may have been throttled while tab was hidden.
        if (timerId !== null)
        {
            clearTimeout(timerId);
        }

        scheduleRefresh();
    }
}

/**
 * Proactive token refresh manager.
 *
 * - Schedules a `setTimeout` to refresh ~2 minutes before the access token expires.
 * - Listens for `visibilitychange` — when the tab becomes visible, checks if the
 *   token is near expiry and triggers an immediate refresh.
 * - Uses raw `fetch` to avoid the axios interceptor (which attaches the expired token
 *   and may trigger its own 401 cycle).
 *
 * Usage:
 *   `TokenRefreshManager.start()` — after login or successful F5 restore.
 *   `TokenRefreshManager.stop()` — on logout.
 */
export const TokenRefreshManager = {
    /** Starts the proactive refresh cycle. Call after login / token restore. */
    start(): void
    {
        TokenRefreshManager.stop();
        scheduleRefresh();

        if (!listening)
        {
            document.addEventListener("visibilitychange", onVisibilityChange);
            listening = true;
        }
    },

    /** Stops all timers and removes the visibility listener. Call on logout. */
    stop(): void
    {
        if (timerId !== null)
        {
            clearTimeout(timerId);
            timerId = null;
        }

        if (listening)
        {
            document.removeEventListener("visibilitychange", onVisibilityChange);
            listening = false;
        }

        refreshing = false;
    },
} as const;
