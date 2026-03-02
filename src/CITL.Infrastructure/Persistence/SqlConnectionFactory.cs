using System.Data;
using CITL.Application.Common.Interfaces;
using CITL.Infrastructure.MultiTenancy;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.Persistence;

/// <summary>
/// Creates tenant-scoped SQL Server connections by reading the current tenant
/// from <see cref="ITenantContext"/> and substituting the <c>{dbName}</c> placeholder
/// in the connection string template.
/// </summary>
/// <param name="tenantContext">The current tenant context.</param>
/// <param name="options">The multi-tenancy settings.</param>
internal sealed class SqlConnectionFactory(
    ITenantContext tenantContext,
    IOptions<TenantSettings> options) : IDbConnectionFactory
{
    private readonly TenantSettings _settings = options.Value;

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        if (!tenantContext.IsResolved)
        {
            throw new TenantException("Tenant context is not resolved. Cannot create a database connection.");
        }

        var connectionString = _settings.ConnectionStringTemplate.Replace(
            TenantConstants.DatabasePlaceholder,
            tenantContext.DatabaseName,
            StringComparison.OrdinalIgnoreCase);

        return new SqlConnection(connectionString);
    }
}
