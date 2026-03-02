using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;

namespace CITL.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring Serilog structured logging.
/// </summary>
internal static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog as the logging provider with enrichers and sinks.
    /// </summary>
    /// <remarks>
    /// <para>Reads configuration from the <c>Serilog</c> section in appsettings.json.</para>
    /// <para>
    /// Sinks: Console (always) + OpenTelemetry OTLP (when <c>OpenTelemetry:Endpoint</c> is configured).
    /// Logs are exported via OTLP gRPC to the Grafana LGTM collector, which routes them to Loki.
    /// </para>
    /// <para>
    /// Default enrichers automatically attached to every log entry:
    /// <list type="bullet">
    ///   <item>CorrelationId — pushed by <see cref="Middleware.CorrelationIdMiddleware"/></item>
    ///   <item>TenantId — pushed by <see cref="Middleware.TenantResolutionMiddleware"/></item>
    ///   <item>MachineName — server hostname</item>
    ///   <item>EnvironmentName — ASPNETCORE_ENVIRONMENT</item>
    ///   <item>ThreadId — managed thread ID</item>
    ///   <item>SpanId / TraceId — OpenTelemetry trace context for log-trace correlation</item>
    /// </list>
    /// </para>
    /// </remarks>
    internal static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Application", Telemetry.DiagnosticsConfig.ServiceName);

            // OpenTelemetry sink — sends structured logs via OTLP to Grafana Loki
            var otlpEndpoint = context.Configuration.GetValue<string>("OpenTelemetry:Endpoint");

            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                configuration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = Telemetry.DiagnosticsConfig.ServiceName,
                        ["service.version"] = Telemetry.DiagnosticsConfig.ServiceVersion
                    };
                });
            }
        });

        return builder;
    }
}
