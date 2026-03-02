namespace CITL.SharedKernel.Constants;

/// <summary>
/// Cross-cutting constants for multi-tenant resolution.
/// Used by middleware, infrastructure, and application layers.
/// </summary>
public static class TenantConstants
{
    /// <summary>
    /// The HTTP request header name carrying the opaque tenant identifier.
    /// Required on every request except those marked with <c>[BypassTenant]</c>.
    /// </summary>
    public const string HeaderName = "X-Tenant-Id";

    /// <summary>
    /// The JWT claim type that carries the tenant identifier.
    /// Used by the tenant guard middleware to cross-validate header vs token.
    /// </summary>
    public const string JwtClaimType = "tenant_id";

    /// <summary>
    /// The placeholder token in the connection string template that is replaced
    /// with the resolved database name at runtime.
    /// </summary>
    public const string DatabasePlaceholder = "{dbName}";
}
