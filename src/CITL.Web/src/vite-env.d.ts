/// <reference types="vite/client" />

/**
 * CSS Modules — strongly typed index signature for *.module.css imports.
 * Allows dot notation access (e.g., styles.page) without noPropertyAccessFromIndexSignature errors.
 */
declare module "*.module.css"
{
    const classes: Record<string, string>;
    export default classes;
}

/**
 * Strongly-typed Vite environment variables.
 * Values come from .env.development / .env.production files.
 */
interface ImportMetaEnv
{
    /** API server URL (e.g. "https://localhost:7001/"). */
    readonly VITE_API_BASE_URL: string;

    /** Base path for client-side routing (e.g. "/" or "/pace/"). */
    readonly VITE_BASE_PATH: string;

    /** Tenant identifier sent in the X-Tenant-Id header on every API request. */
    readonly VITE_CLIENT_NAME: string;
}

/** Build-time app version injected by Vite define. */
declare const __APP_VERSION__: string;

interface ImportMeta
{
    readonly env: ImportMetaEnv;
}
