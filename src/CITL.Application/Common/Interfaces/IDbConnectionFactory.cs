using System.Data;

namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Creates tenant-scoped database connections.
/// The implementation reads the current tenant from <see cref="ITenantContext"/>
/// and substitutes <c>{dbName}</c> in the connection string template.
/// </summary>
/// <remarks>
/// Always wrap the returned connection in a <c>using</c> declaration.
/// Never hold connections open across multiple operations.
/// <code>
/// using var connection = _connectionFactory.CreateConnection();
/// var users = await connection.QueryAsync&lt;User&gt;(sql, parameters);
/// </code>
/// </remarks>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> scoped to the current tenant's database.
    /// </summary>
    /// <returns>A new, unopened database connection.</returns>
    /// <exception cref="SharedKernel.Exceptions.TenantException">
    /// Thrown when the tenant context has not been resolved.
    /// </exception>
    IDbConnection CreateConnection();
}
