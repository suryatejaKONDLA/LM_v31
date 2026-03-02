namespace CITL.WebApi.Configuration;

/// <summary>
/// Strongly-typed settings for CORS policy.
/// Bound from the <c>Cors</c> section in appsettings.json.
/// </summary>
public sealed class CorsSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// The CORS policy name used by the middleware and endpoint metadata.
    /// </summary>
    public const string PolicyName = "CorsPolicy";

    /// <summary>
    /// Allowed origin URLs (e.g. "https://localhost:5173", "https://app.citl.co.in").
    /// An empty array disallows all origins.
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Allowed HTTP methods (e.g. "GET", "POST"). Defaults to all methods if empty.
    /// </summary>
    public string[] AllowedMethods { get; init; } = [];

    /// <summary>
    /// Allowed HTTP request headers. Defaults to all headers if empty.
    /// </summary>
    public string[] AllowedHeaders { get; init; } = [];

    /// <summary>
    /// Headers exposed to the client in the response. Empty means none exposed.
    /// </summary>
    public string[] ExposedHeaders { get; init; } = [];

    /// <summary>
    /// Whether the browser should include credentials (cookies, auth headers) in cross-origin requests.
    /// </summary>
    public bool AllowCredentials { get; init; }
}
