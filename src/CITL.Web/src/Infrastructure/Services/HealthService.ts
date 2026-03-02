import { AppConfig } from "@/Shared/Index";

// ── Response types matching HealthCheckResponseWriter.cs ──────────────────────

export type HealthStatus = "Healthy" | "Degraded" | "Unhealthy";

export interface ServiceHealthEntry
{
    Name: string;
    Status: HealthStatus;
    Description: string | null;
    DurationMs: number;
    Error: string | null;
    Data: Record<string, unknown> | null;
}

export interface HealthCheckResponse
{
    Status: HealthStatus;
    TotalDurationMs: number;
    Timestamp: string;
    Services: ServiceHealthEntry[];
}

// ── Helpers ──────────────────────────────────────────────────────────────────

async function fetchHealthJson<T>(path: string, signal?: AbortSignal): Promise<T>
{
    const response = await fetch(`${AppConfig.ApiBaseUrl}${path}`, { signal: signal ?? null });
    return response.json() as Promise<T>;
}

/**
 * Health API service.
 * Uses fetch directly — /Health returns raw JSON (not wrapped in ApiResponse)
 * and may return HTTP 503 for unhealthy status.
 */
export const HealthService =
{
    getHealth(signal?: AbortSignal): Promise<HealthCheckResponse>
    {
        return fetchHealthJson<HealthCheckResponse>("Health", signal);
    },

    getLive(signal?: AbortSignal): Promise<{ Status: string }>
    {
        return fetchHealthJson<{ Status: string }>("Health/Live", signal);
    },
} as const;
