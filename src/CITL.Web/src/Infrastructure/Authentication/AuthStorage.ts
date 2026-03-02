const ACCESS_TOKEN_KEY = "citl_access_token";
const REFRESH_TOKEN_KEY = "citl_refresh_token";
const LOGIN_USER_KEY = "citl_login_user";
const EXPIRES_AT_KEY = "citl_expires_at";

/** Buffer in milliseconds — refresh 2 minutes before expiry. */
const REFRESH_BUFFER_MS = 2 * 60 * 1000;

/**
 * Manages JWT token persistence in localStorage.
 * Mirrors auth storage patterns from the old React app's AuthStorageHelpers.
 */
export const AuthStorage = {
    getAccessToken(): string | null
    {
        return localStorage.getItem(ACCESS_TOKEN_KEY);
    },

    getRefreshToken(): string | null
    {
        return localStorage.getItem(REFRESH_TOKEN_KEY);
    },

    getLoginUser(): string | null
    {
        return localStorage.getItem(LOGIN_USER_KEY);
    },

    setTokens(accessToken: string, refreshToken: string): void
    {
        localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
        localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);

        // Decode and persist expiry from the JWT
        const exp = AuthStorage.getTokenExpiry(accessToken);

        if (exp)
        {
            localStorage.setItem(EXPIRES_AT_KEY, exp.toString());
        }
    },

    setLoginUser(loginUser: string): void
    {
        localStorage.setItem(LOGIN_USER_KEY, loginUser);
    },

    clear(): void
    {
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        localStorage.removeItem(LOGIN_USER_KEY);
        localStorage.removeItem(EXPIRES_AT_KEY);
    },

    /** Checks if a token exists AND is not expired (with buffer). */
    isAuthenticated(): boolean
    {
        const token = localStorage.getItem(ACCESS_TOKEN_KEY);

        if (token === null || token.length === 0)
        {
            return false;
        }

        // If we have a stored expiry, check it
        const expiresAt = AuthStorage.getExpiresAtMs();

        if (expiresAt !== null)
        {
            return Date.now() < expiresAt;
        }

        // Fallback — token exists but no expiry stored (shouldn't happen)
        return true;
    },

    /** Returns true if the token is within the refresh buffer window or already expired. */
    isTokenNearExpiry(): boolean
    {
        const expiresAt = AuthStorage.getExpiresAtMs();

        if (expiresAt === null)
        {
            return false;
        }

        return Date.now() >= expiresAt - REFRESH_BUFFER_MS;
    },

    /** Returns the absolute expiry time in ms, or null. */
    getExpiresAtMs(): number | null
    {
        const stored = localStorage.getItem(EXPIRES_AT_KEY);

        if (stored === null)
        {
            return null;
        }

        const ms = Number(stored);
        return Number.isFinite(ms) ? ms : null;
    },

    /** Returns milliseconds until the access token expires, or 0 if expired/unknown. */
    getTimeUntilExpiryMs(): number
    {
        const expiresAt = AuthStorage.getExpiresAtMs();

        if (expiresAt === null)
        {
            return 0;
        }

        return Math.max(0, expiresAt - Date.now());
    },

    /**
     * Decodes the `exp` claim from a JWT payload (base64url).
     * Returns the expiry as epoch milliseconds, or null on failure.
     */
    getTokenExpiry(token: string): number | null
    {
        try
        {
            const parts = token.split(".");

            if (parts.length !== 3)
            {
                return null;
            }

            // Base64url → Base64 → decode
            const payload = parts[1];

            if (!payload)
            {
                return null;
            }

            const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
            const json = atob(base64);
            const parsed = JSON.parse(json) as { exp?: number };

            if (typeof parsed.exp === "number")
            {
                return parsed.exp * 1000; // seconds → ms
            }

            return null;
        }
        catch
        {
            return null;
        }
    },
} as const;
