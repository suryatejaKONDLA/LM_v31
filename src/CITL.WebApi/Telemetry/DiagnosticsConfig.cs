using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CITL.WebApi.Telemetry;

/// <summary>
/// Centralized diagnostic configuration for application telemetry.
/// Defines meters, counters, histograms, and activity sources used across the application.
/// </summary>
public static class DiagnosticsConfig
{
    /// <summary>Service name used for telemetry identification.</summary>
    public const string ServiceName = "CITL.WebApi";

    /// <summary>Service version for telemetry resource tagging.</summary>
    public const string ServiceVersion = "1.0.0";

    /// <summary>Activity source name used by <c>DbExecutor</c> for SQL query tracing.</summary>
    public const string DatabaseSourceName = "CITL.Database";

    /// <summary>Meter name used by <c>DbExecutor</c> for SQL query metrics.</summary>
    public const string DatabaseMeterName = "CITL.Database";

    // -----------------------------------------------------------------------
    // Activity sources (distributed tracing)
    // -----------------------------------------------------------------------

    /// <summary>Activity source for WebApi-level spans.</summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    // -----------------------------------------------------------------------
    // HTTP metrics
    // -----------------------------------------------------------------------

    /// <summary>Meter for custom application metrics.</summary>
    public static readonly Meter RequestMeter = new(ServiceName, ServiceVersion);

    /// <summary>Counter for total HTTP requests processed.</summary>
    /// <remarks>Metric name: <c>citl.http.requests</c>.</remarks>
    public static readonly Counter<long> RequestCounter =
        RequestMeter.CreateCounter<long>(
            "citl.http.requests",
            unit: null,
            "Total HTTP requests processed");

    /// <summary>Histogram for HTTP request duration in milliseconds.</summary>
    /// <remarks>Metric name: <c>citl.http.request.duration</c>.</remarks>
    public static readonly Histogram<double> RequestDuration =
        RequestMeter.CreateHistogram<double>(
            "citl.http.request.duration",
            "ms",
            "HTTP request duration in milliseconds");

    /// <summary>Counter for HTTP server errors (5xx).</summary>
    /// <remarks>Metric name: <c>citl.http.errors</c>.</remarks>
    public static readonly Counter<long> ErrorCounter =
        RequestMeter.CreateCounter<long>(
            "citl.http.errors",
            unit: null,
            "Total HTTP server errors (5xx)");
}
