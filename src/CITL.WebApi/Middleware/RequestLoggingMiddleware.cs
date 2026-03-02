using System.Diagnostics;
using CITL.Application.Common.Interfaces;
using CITL.WebApi.Telemetry;

namespace CITL.WebApi.Middleware;

/// <summary>
/// Logs the start and completion of every HTTP request with structured properties
/// including method, path, status code, duration, and correlation ID.
/// </summary>
/// <remarks>
/// <para>
/// Also records custom metrics:
/// <list type="bullet">
///   <item><c>citl.http.requests.total</c> — counter of all processed requests</item>
///   <item><c>citl.http.requests.duration</c> — histogram of request durations (ms)</item>
///   <item><c>citl.http.errors.total</c> — counter of 5xx server errors</item>
/// </list>
/// </para>
/// <para>
/// <b>Pipeline order</b>: CorrelationId → <b>RequestLogging</b> → GlobalException → …
/// </para>
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger.</param>
public sealed partial class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        LogRequestStarted(logger, method, path);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            var statusCode = context.Response.StatusCode;

            LogRequestCompleted(logger, method, path, statusCode, elapsedMs);

            // Resolve tenant (set by TenantResolutionMiddleware which runs inside next())
            var tenant = ResolveTenant(context);

            // Record application metrics
            RecordMetrics(method, path, statusCode, elapsedMs, tenant);
        }
    }

    private static string ResolveTenant(HttpContext context)
    {
        var services = context.RequestServices;

        if (services is null)
        {
            return "unknown";
        }

        var tenantContext = services.GetService<ITenantContext>();
        return tenantContext is not null && tenantContext.IsResolved
            ? tenantContext.TenantId
            : "unknown";
    }

    private static void RecordMetrics(string method, string path, int statusCode, double elapsedMs, string tenant)
    {
        var tags = new TagList
        {
            { "http.request.method", method },
            { "http.route", path },
            { "http.response.status_code", statusCode },
            { "tenant", tenant }
        };

        DiagnosticsConfig.RequestCounter.Add(1, tags);
        DiagnosticsConfig.RequestDuration.Record(elapsedMs, tags);

        if (statusCode >= 500)
        {
            DiagnosticsConfig.ErrorCounter.Add(1, tags);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} started")]
    private static partial void LogRequestStarted(
        ILogger logger, string method, string path);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} completed | {StatusCode} | {ElapsedMs:F1}ms")]
    private static partial void LogRequestCompleted(
        ILogger logger, string method, string path, int statusCode, double elapsedMs);
}
