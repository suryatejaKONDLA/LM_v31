using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Guards;

namespace CITL.Infrastructure.MultiTenancy;

/// <summary>
/// Scoped implementation of <see cref="ITenantContext"/>.
/// Each HTTP request gets its own instance via DI.
/// <c>TenantResolutionMiddleware</c> sets the tenant; services and repositories read it.
/// </summary>
internal sealed class TenantContext : ITenantContext
{
    /// <inheritdoc />
    public string TenantId { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string DatabaseName { get; private set; } = string.Empty;

    /// <inheritdoc />
    public bool IsResolved => !string.IsNullOrEmpty(TenantId);

    /// <inheritdoc />
    public void SetTenant(string tenantId, string databaseName)
    {
        Guard.NotNullOrWhiteSpace(tenantId);
        Guard.NotNullOrWhiteSpace(databaseName);

        TenantId = tenantId;
        DatabaseName = databaseName;
    }
}
