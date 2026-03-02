namespace CITL.WebApi.Configuration;

/// <summary>
/// Strongly-typed settings for API documentation (Swagger UI &amp; Scalar).
/// Bound from the <c>ApiDocumentation</c> section in appsettings.json.
/// </summary>
public sealed class ApiDocumentationSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "ApiDocumentation";

    /// <summary>
    /// API version label shown in the documentation header.
    /// </summary>
    public string Version { get; init; } = "v1";

    /// <summary>
    /// Title displayed in Swagger UI and Scalar.
    /// </summary>
    public string Title { get; init; } = "CITL API";

    /// <summary>
    /// Short description of the API.
    /// </summary>
    public string Description { get; init; } = "CITL Multi-Tenant SaaS Platform API";

    /// <summary>
    /// Contact name shown in the documentation.
    /// </summary>
    public string ContactName { get; init; } = string.Empty;

    /// <summary>
    /// Contact email shown in the documentation.
    /// </summary>
    public string ContactEmail { get; init; } = string.Empty;

    /// <summary>
    /// Contact URL shown in the documentation.
    /// </summary>
    public string ContactUrl { get; init; } = string.Empty;

    /// <summary>
    /// When true, generates a separate OpenAPI document per module group.
    /// When false, a single combined document is generated.
    /// </summary>
    public bool UseMultipleApiGroups { get; init; } = true;
}
