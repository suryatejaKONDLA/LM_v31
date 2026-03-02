using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks Grafana dashboard availability by calling its /api/health endpoint.
/// Skips when the OpenTelemetry endpoint is not configured.
/// </summary>
internal sealed class GrafanaHealthCheck(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IHealthCheck
{
    private const string GrafanaPort = "3000";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        var otlpEndpoint = configuration["OpenTelemetry:Endpoint"];

        // If OTLP is not configured, Grafana is not expected
        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            data["Reason"] = "OpenTelemetry endpoint not configured; Grafana check skipped.";
            return HealthCheckResult.Healthy("Grafana check skipped — OTLP not configured.", data);
        }

        // Derive Grafana URL from OTLP endpoint host
        var grafanaUrl = BuildGrafanaUrl(otlpEndpoint);
        data["GrafanaUrl"] = grafanaUrl;

        try
        {
            var client = httpClientFactory.CreateClient("HealthChecks");
            client.Timeout = TimeSpan.FromSeconds(5);

            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(
                $"{grafanaUrl}/api/health",
                cancellationToken).ConfigureAwait(false);
            sw.Stop();

            data["ResponseTimeMs"] = sw.ElapsedMilliseconds;
            data["StatusCode"] = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Grafana is responsive.", data);
            }

            return HealthCheckResult.Degraded(
                $"Grafana returned HTTP {(int)response.StatusCode}.",
                data: data);
        }
        catch (TaskCanceledException)
        {
            data["Error"] = "Request timed out after 5 seconds.";
            return HealthCheckResult.Unhealthy("Grafana is unreachable (timeout).", data: data);
        }
        catch (HttpRequestException ex)
        {
            data["Error"] = ex.Message;
            return HealthCheckResult.Unhealthy("Grafana is unreachable.", ex, data);
        }
    }

    private static string BuildGrafanaUrl(string otlpEndpoint)
    {
        // OTLP endpoint is like "http://localhost:4317" — Grafana runs on same host, port 3000
        if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var uri))
        {
            return $"{uri.Scheme}://{uri.Host}:{GrafanaPort}";
        }

        return $"http://localhost:{GrafanaPort}";
    }
}
