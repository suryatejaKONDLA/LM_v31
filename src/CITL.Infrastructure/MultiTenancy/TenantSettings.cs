namespace CITL.Infrastructure.MultiTenancy;

/// <summary>
/// Configuration POCO for multi-tenant settings.
/// Bound from <c>appsettings.json</c> section <c>"MultiTenancy"</c>.
/// </summary>
/// <example>
/// <code>
/// "MultiTenancy": {
///   "ConnectionStringTemplate": "Server=.;Database={dbName};Trusted_Connection=true;TrustServerCertificate=true",
///   "TenantMappings": {
///     "tn_a7f2c9e4b8d1": "CITL_Prod",
///     "tn_dev_001": "CITL_Dev"
///   }
/// }
/// </code>
/// </example>
public sealed class TenantSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "MultiTenancy";

    /// <summary>
    /// Gets the connection string template with <c>{dbName}</c> placeholder.
    /// The placeholder is replaced at runtime with the resolved database name.
    /// </summary>
    public string ConnectionStringTemplate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the opaque tenant identifier to database name mappings.
    /// Key = opaque tenant ID (from <c>X-Tenant-Id</c> header), Value = database name.
    /// </summary>
    /// <remarks>
    /// Tenant IDs should be opaque, non-guessable identifiers (e.g., <c>tn_a7f2c9e4b8d1</c>).
    /// Database names are never exposed to clients.
    /// </remarks>
    public Dictionary<string, string> TenantMappings { get; init; } = [];
}
