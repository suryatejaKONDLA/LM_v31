using CITL.SharedKernel.Constants;
using Serilog.Context;

namespace CITL.WebApi.Middleware;

/// <summary>
/// Ensures every HTTP request has a unique correlation identifier for distributed tracing.
/// </summary>
/// <remarks>
/// <para>
/// If the client sends an <c>X-Correlation-Id</c> header, that value is reused to preserve
/// end-to-end trace continuity. Otherwise a new GUID is generated.
/// </para>
/// <para>
/// The correlation ID is:
/// <list type="bullet">
///   <item>Stored in <see cref="HttpContext.TraceIdentifier"/> for built-in integration</item>
///   <item>Returned in the <c>X-Correlation-Id</c> response header</item>
///   <item>Pushed into a logging scope so all downstream log entries include it</item>
/// </list>
/// </para>
/// <para>
/// <b>Pipeline order</b>: <b>CorrelationId</b> → RequestLogging → GlobalException → …
/// </para>
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Override the built-in TraceIdentifier so all ASP.NET Core logs use it
        context.TraceIdentifier = correlationId;

        // Set correlation ID response header eagerly — safe because the response
        // has not started yet at this point in the pipeline.
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

        // Push correlation ID into Serilog's ambient LogContext so ALL loggers
        // (across every service/repository in this request) include it automatically.
        using (LogContext.PushProperty(CorrelationConstants.LogPropertyName, correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationConstants.HeaderName, out var values))
        {
            var headerValue = values.ToString();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return Guid.NewGuid().ToString("D");
    }
}
