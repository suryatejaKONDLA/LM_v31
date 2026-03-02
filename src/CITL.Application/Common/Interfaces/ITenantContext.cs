namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current tenant context within a request scope.
/// Registered as <b>Scoped</b> in DI — each HTTP request gets its own instance.
/// </summary>
/// <remarks>
/// The tenant is resolved by <c>TenantResolutionMiddleware</c> (reads <c>X-Tenant-Id</c>
/// header, resolves via <see cref="ITenantRegistry"/>) and set via <see cref="SetTenant"/>.
/// All downstream services and repositories read tenant info from this context.
/// <para>
/// <b>Never</b> use AsyncLocal, ThreadLocal, or static fields for tenant state.
/// </para>
/// </remarks>
public interface ITenantContext
{
    /// <summary>
    /// Gets the opaque tenant identifier extracted from the <c>X-Tenant-Id</c> request header.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the database name resolved from the tenant identifier via <see cref="ITenantRegistry"/>.
    /// </summary>
    string DatabaseName { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant has been resolved for this request.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Sets the tenant for the current scope. Called by tenant resolution middleware.
    /// </summary>
    /// <param name="tenantId">The opaque tenant identifier from the request header.</param>
    /// <param name="databaseName">The database name resolved from the tenant registry.</param>
    void SetTenant(string tenantId, string databaseName);
}
