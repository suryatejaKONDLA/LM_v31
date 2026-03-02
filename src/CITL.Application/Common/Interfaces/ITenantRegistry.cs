using System.Diagnostics.CodeAnalysis;

namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Resolves opaque tenant identifiers (from <c>X-Tenant-Id</c> header)
/// to database names. Implementations should be registered as singletons
/// for maximum performance using frozen collections.
/// </summary>
public interface ITenantRegistry
{
    /// <summary>
    /// Attempts to resolve a database name from an opaque tenant identifier.
    /// </summary>
    /// <param name="tenantId">The opaque tenant identifier from the request header.</param>
    /// <param name="databaseName">When successful, the resolved database name; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the tenant exists and is mapped; otherwise, <see langword="false"/>.</returns>
    bool TryGetDatabaseName(string tenantId, [NotNullWhen(true)] out string? databaseName);

    /// <summary>
    /// Returns all configured tenant identifiers.
    /// Useful for diagnostics and health checks.
    /// </summary>
    /// <returns>A read-only collection of all known tenant identifiers.</returns>
    IReadOnlyCollection<string> GetAllTenantIds();
}
