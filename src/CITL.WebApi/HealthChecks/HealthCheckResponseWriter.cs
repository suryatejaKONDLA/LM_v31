using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CITL.WebApi.HealthChecks;

/// <summary>
/// Writes a structured JSON response for health check endpoints.
/// Designed for frontend consumption with service-level status, response times,
/// and overall system health with degradation indicators.
/// </summary>
internal static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null, // PascalCase to match API convention
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    /// <summary>
    /// Writes a detailed JSON health report to the HTTP response.
    /// </summary>
    public static async Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            TotalDurationMs = report.TotalDuration.TotalMilliseconds,
            Timestamp = DateTime.UtcNow,
            Services = report.Entries.Select(entry => new ServiceHealthEntry
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                DurationMs = entry.Value.Duration.TotalMilliseconds,
                Error = entry.Value.Exception?.Message,
                Data = entry.Value.Data.Count > 0
                    ? entry.Value.Data.ToDictionary<KeyValuePair<string, object>, string, object?>(d => d.Key, d => d.Value)
                    : null
            }).ToList()
        };

        // Set HTTP status code based on overall health
        httpContext.Response.StatusCode = report.Status switch
        {
            HealthStatus.Healthy => StatusCodes.Status200OK,
            HealthStatus.Degraded => StatusCodes.Status200OK,
            HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status200OK
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions)).ConfigureAwait(false);
    }
}

/// <summary>
/// Top-level health check response — designed for frontend rendering.
/// </summary>
internal sealed class HealthCheckResponse
{
    /// <summary>Overall system status: Healthy, Degraded, or Unhealthy.</summary>
    public required string Status { get; init; }

    /// <summary>Total time taken for all health checks in milliseconds.</summary>
    public required double TotalDurationMs { get; init; }

    /// <summary>UTC timestamp of the health check.</summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>Individual service health entries.</summary>
    public required List<ServiceHealthEntry> Services { get; init; }
}

/// <summary>
/// Individual service health entry within the health check response.
/// </summary>
internal sealed class ServiceHealthEntry
{
    /// <summary>Service name (e.g., "SqlServer", "Redis", "R2Storage").</summary>
    public required string Name { get; init; }

    /// <summary>Service status: Healthy, Degraded, or Unhealthy.</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable description of the health status.</summary>
    public string? Description { get; init; }

    /// <summary>Time taken for this specific check in milliseconds.</summary>
    public required double DurationMs { get; init; }

    /// <summary>Error message if the check failed.</summary>
    public string? Error { get; init; }

    /// <summary>Additional data reported by the check (e.g., per-tenant DB status, disk space).</summary>
    public Dictionary<string, object?>? Data { get; init; }
}
