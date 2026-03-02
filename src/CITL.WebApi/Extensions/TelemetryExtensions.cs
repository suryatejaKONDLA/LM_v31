using CITL.WebApi.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CITL.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry tracing and metrics
/// with OTLP export to the Grafana LGTM stack.
/// </summary>
internal static class TelemetryExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing and metrics with OTLP exporters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tracing instruments: ASP.NET Core, HttpClient, SqlClient, and custom
    /// <see cref="DiagnosticsConfig.ServiceName"/> / <see cref="DiagnosticsConfig.DatabaseSourceName"/>
    /// activity sources.
    /// </para>
    /// <para>
    /// Metrics instruments: ASP.NET Core, HttpClient, .NET Runtime, and custom
    /// <see cref="DiagnosticsConfig.ServiceName"/> / <see cref="DiagnosticsConfig.DatabaseMeterName"/> meters.
    /// </para>
    /// <para>
    /// All telemetry is exported via OTLP (gRPC) to the endpoint in
    /// <c>OpenTelemetry:Endpoint</c> (default: <c>http://localhost:4317</c>).
    /// </para>
    /// </remarks>
    internal static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:Endpoint")
            ?? "http://localhost:4317";
        var otlpUri = new Uri(otlpEndpoint);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: DiagnosticsConfig.ServiceName,
                    serviceVersion: DiagnosticsConfig.ServiceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = httpContext =>
                    {
                        // Skip health check and documentation endpoints
                        var path = httpContext.Request.Path.Value;
                        return path is not null
                            && !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);
                    };
                })
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddSource(DiagnosticsConfig.ServiceName)
                .AddSource(DiagnosticsConfig.DatabaseSourceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpUri;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(DiagnosticsConfig.ServiceName)
                .AddMeter(DiagnosticsConfig.DatabaseMeterName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpUri;
                }));

        return services;
    }
}
