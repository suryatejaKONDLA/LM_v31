namespace CITL.SharedKernel.Constants;

/// <summary>
/// Cross-cutting constants for request correlation and distributed tracing.
/// Used by middleware, infrastructure, and application layers.
/// </summary>
public static class CorrelationConstants
{
    /// <summary>
    /// The HTTP header name carrying the correlation identifier for distributed tracing.
    /// Clients may send this header to propagate correlation across service boundaries;
    /// otherwise the server generates one automatically.
    /// </summary>
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>
    /// The structured-logging property name for the correlation identifier.
    /// All log entries within a request scope include this property.
    /// </summary>
    public const string LogPropertyName = "CorrelationId";
}
