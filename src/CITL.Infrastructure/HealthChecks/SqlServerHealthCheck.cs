using System.Data;
using CITL.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks SQL Server connectivity for every configured tenant database.
/// Reports individual tenant status in <see cref="HealthCheckResult.Data"/>.
/// </summary>
internal sealed class SqlServerHealthCheck(
    ITenantRegistry tenantRegistry,
    IOptions<MultiTenancy.TenantSettings> options) : IHealthCheck
{
    private const string TestQuery = "SELECT 1;";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var tenantIds = tenantRegistry.GetAllTenantIds();
        var data = new Dictionary<string, object>();
        var allHealthy = true;
        var anyHealthy = false;

        foreach (var tenantId in tenantIds)
        {
            if (!tenantRegistry.TryGetDatabaseName(tenantId, out var dbName))
            {
                data[tenantId] = new { Status = "Unknown", Error = "Tenant not mapped" };
                allHealthy = false;
                continue;
            }

            var connectionString = options.Value.ConnectionStringTemplate.Replace(
                SharedKernel.Constants.TenantConstants.DatabasePlaceholder,
                dbName,
                StringComparison.OrdinalIgnoreCase);

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using var command = connection.CreateCommand();
                command.CommandText = TestQuery;
                command.CommandType = CommandType.Text;
                await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                sw.Stop();

                data[tenantId] = new { Status = "Healthy", ResponseTimeMs = sw.ElapsedMilliseconds };
                anyHealthy = true;
            }
            catch (Exception ex)
            {
                data[tenantId] = new { Status = "Unhealthy", Error = ex.Message };
                allHealthy = false;
            }
        }

        if (allHealthy)
        {
            return HealthCheckResult.Healthy("All tenant databases are reachable.", data);
        }

        if (anyHealthy)
        {
            return HealthCheckResult.Degraded("Some tenant databases are unreachable.", data: data);
        }

        return HealthCheckResult.Unhealthy("No tenant databases are reachable.", data: data);
    }
}
