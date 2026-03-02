using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks OpenTelemetry Collector connectivity on both gRPC (4317) and HTTP (4318) ports.
/// Skips when the OpenTelemetry endpoint is not configured.
/// </summary>
internal sealed class OtlpCollectorHealthCheck(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IHealthCheck
{
    private const int GrpcPort = 4317;
    private const int HttpPort = 4318;

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        var otlpEndpoint = configuration["OpenTelemetry:Endpoint"];

        // If OTLP is not configured, collector is not expected
        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            data["Reason"] = "OpenTelemetry endpoint not configured; collector check skipped.";
            return HealthCheckResult.Healthy("OTLP Collector check skipped — not configured.", data);
        }

        data["ConfiguredEndpoint"] = otlpEndpoint;

        var host = "localhost";
        if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var uri))
        {
            host = uri.Host;
        }

        var grpcOk = false;
        var httpOk = false;

        // Check gRPC port (4317) via TCP
        try
        {
            var sw = Stopwatch.StartNew();
            using var tcp = new System.Net.Sockets.TcpClient();
            await tcp.ConnectAsync(host, GrpcPort, cancellationToken).ConfigureAwait(false);
            sw.Stop();
            grpcOk = true;
            data["gRPC_Port"] = GrpcPort;
            data["gRPC_Status"] = "Connected";
            data["gRPC_ResponseTimeMs"] = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            data["gRPC_Port"] = GrpcPort;
            data["gRPC_Status"] = "Failed";
            data["gRPC_Error"] = ex.Message;
        }

        // Check HTTP port (4318) via HTTP GET
        try
        {
            var client = httpClientFactory.CreateClient("HealthChecks");
            client.Timeout = TimeSpan.FromSeconds(5);

            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(
                $"http://{host}:{HttpPort}",
                cancellationToken).ConfigureAwait(false);
            sw.Stop();
            httpOk = true;
            data["HTTP_Port"] = HttpPort;
            data["HTTP_Status"] = "Connected";
            data["HTTP_StatusCode"] = (int)response.StatusCode;
            data["HTTP_ResponseTimeMs"] = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            data["HTTP_Port"] = HttpPort;
            data["HTTP_Status"] = "Failed";
            data["HTTP_Error"] = ex.Message;
        }

        if (grpcOk && httpOk)
        {
            return HealthCheckResult.Healthy("OTLP Collector is fully responsive (gRPC + HTTP).", data);
        }

        if (grpcOk || httpOk)
        {
            var working = grpcOk ? "gRPC" : "HTTP";
            return HealthCheckResult.Degraded(
                $"OTLP Collector partially available — {working} port is responding.",
                data: data);
        }

        return HealthCheckResult.Unhealthy("OTLP Collector is unreachable on both gRPC and HTTP ports.", data: data);
    }
}
