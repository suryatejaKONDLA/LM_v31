/**
 * Resolved application configuration from environment variables.
 * Single access point for all env config — no scattered import.meta.env reads.
 *
 * Validated at startup in development mode — fails fast on missing required values.
 * Frozen after creation — immutable at runtime.
 */

interface AppConfiguration
{
    /** API server base URL (always ends with "/"). */
    readonly ApiBaseUrl: string;

    /** Client-side routing base path (e.g. "/" or "/myapp/"). */
    readonly BasePath: string;

    /**
     * Tenant identifier sent in the X-Tenant-Id header on every API request.
     * Must match a key in the backend MultiTenancy.TenantMappings config.
     */
    readonly TenantId: string;

    /** True when running in production mode. */
    readonly IsProduction: boolean;

    /** True when running in development mode. */
    readonly IsDevelopment: boolean;
}

function ensureTrailingSlash(url: string): string
{
    return url.endsWith("/") ? url : `${url}/`;
}

/**
 * Normalizes a raw base-path value (e.g. "pace", "/pace/", "/pace") into
 * the format React Router expects: leading slash, no trailing slash ("/pace").
 * Returns "/" when the value is empty or missing.
 */
function normalizeBasePath(raw: string | undefined): string
{
    if (!raw)
    {
        return "/";
    }

    const trimmed = raw.trim().replace(/^\/+/, "").replace(/\/+$/, "");

    return trimmed.length > 0 ? `/${trimmed}` : "/";
}

function resolveConfig(): AppConfiguration
{
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
    const basePath = import.meta.env.VITE_BASE_PATH;
    const tenantId = import.meta.env.VITE_CLIENT_NAME;

    // Fail fast in development if required config is missing
    if (import.meta.env.DEV)
    {
        if (!apiBaseUrl)
        {
            throw new Error("Missing required env: VITE_API_BASE_URL — see .env.example");
        }

        if (!tenantId)
        {
            throw new Error("Missing required env: VITE_CLIENT_NAME — see .env.example");
        }
    }

    return Object.freeze<AppConfiguration>({
        ApiBaseUrl: ensureTrailingSlash(apiBaseUrl || "/"),
        BasePath: normalizeBasePath(basePath),
        TenantId: tenantId || "",
        IsProduction: import.meta.env.PROD,
        IsDevelopment: import.meta.env.DEV,
    });
}

/**
 * Immutable app config resolved once at startup.
 * Import this instead of reading import.meta.env directly.
 */
export const AppConfig: AppConfiguration = resolveConfig();
