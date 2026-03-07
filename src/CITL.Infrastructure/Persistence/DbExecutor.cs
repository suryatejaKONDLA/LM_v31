using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CITL.Infrastructure.Persistence;

/// <summary>
/// Dapper implementation of <see cref="IDbExecutor"/>.
/// Manages connection lifecycle, parameter binding, and SP output extraction
/// so that repositories contain zero infrastructure boilerplate.
/// </summary>
/// <remarks>
/// Each database operation is instrumented with:
/// <list type="bullet">
///   <item><see cref="Activity"/> spans for distributed tracing (visible in Grafana Tempo)</item>
///   <item>Metrics (counter + histogram) for query rate and duration (visible in Grafana/Prometheus)</item>
///   <item>Structured Serilog logs for every query (visible in Grafana Loki)</item>
/// </list>
/// </remarks>
internal sealed partial class DbExecutor(
    IDbConnectionFactory connectionFactory,
    ITenantContext tenantContext,
    ILogger<DbExecutor> logger) : IDbExecutor
{
    // -----------------------------------------------------------------------
    // Telemetry — same source/meter names registered in TelemetryExtensions
    // -----------------------------------------------------------------------

    private static readonly ActivitySource DbActivity = new("CITL.Database");

    private static readonly Meter DbMeter = new("CITL.Database");

    private static readonly Counter<long> DbQueryCounter =
        DbMeter.CreateCounter<long>(
            "citl.db.queries",
            unit: null,
            "Total database queries executed");

    private static readonly Histogram<double> DbQueryDuration =
        DbMeter.CreateHistogram<double>(
            "citl.db.query.duration",
            "ms",
            "Database query duration in milliseconds");

    private string CurrentTenant => tenantContext.IsResolved ? tenantContext.TenantId : "unknown";

    /// <inheritdoc />
    public async Task<SpResult> ExecuteSpAsync(
        string storedProcedure,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var tenant = CurrentTenant;

        using var activity = DbActivity.StartActivity("DB " + storedProcedure);
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.operation.name", "ExecuteStoredProcedure");
        activity?.SetTag("db.query.text", storedProcedure);
        activity?.SetTag("tenant", tenant);

        var sw = Stopwatch.StartNew();

        try
        {
            var dp = new DynamicParameters();

            foreach (var (key, value) in parameters)
            {
                dp.Add($"@{key}", value);
            }

            // Standard CITL SP output contract
            dp.Add("@ResultVal", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@ResultType", dbType: DbType.AnsiString, direction: ParameterDirection.Output, size: 10);
            dp.Add("@ResultMessage", dbType: DbType.AnsiString, direction: ParameterDirection.Output, size: 4000);

            using var connection = connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                storedProcedure,
                dp,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command).ConfigureAwait(false);

            var result = new SpResult
            {
                ResultVal = dp.Get<int>("@ResultVal"),
                ResultType = dp.Get<string>("@ResultType"),
                ResultMessage = dp.Get<string>("@ResultMessage")
            };

            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Ok);

            LogStoredProcedureExecuted(logger, tenant, FormatSpExec(storedProcedure, parameters), sw.Elapsed.TotalMilliseconds, result.ResultVal, result.ResultType);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            LogDatabaseError(logger, tenant, "StoredProcedure", FormatSpExec(storedProcedure, parameters), sw.Elapsed.TotalMilliseconds, ex);

            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "db.operation", "stored_procedure" },
                { "db.name", storedProcedure },
                { "tenant", tenant }
            };
            DbQueryDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            DbQueryCounter.Add(1, tags);
        }
    }

    /// <inheritdoc />
    public async Task<(SpResult SpResult, string ExtraValue)> ExecuteSpAsync(
        string storedProcedure,
        Dictionary<string, object?> parameters,
        string extraOutputParamName,
        CancellationToken cancellationToken = default)
    {
        var tenant = CurrentTenant;

        using var activity = DbActivity.StartActivity("DB " + storedProcedure);
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.operation.name", "ExecuteStoredProcedure");
        activity?.SetTag("db.query.text", storedProcedure);
        activity?.SetTag("tenant", tenant);

        var sw = Stopwatch.StartNew();

        try
        {
            var dp = new DynamicParameters();

            foreach (var (key, value) in parameters)
            {
                dp.Add($"@{key}", value);
            }

            dp.Add("@ResultVal", dbType: DbType.Int32, direction: ParameterDirection.Output);
            dp.Add("@ResultType", dbType: DbType.AnsiString, direction: ParameterDirection.Output, size: 10);
            dp.Add("@ResultMessage", dbType: DbType.AnsiString, direction: ParameterDirection.Output, size: 4000);
            dp.Add($"@{extraOutputParamName}", dbType: DbType.AnsiString, direction: ParameterDirection.Output, size: 500);

            using var connection = connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                storedProcedure,
                dp,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command).ConfigureAwait(false);

            var spResult = new SpResult
            {
                ResultVal = dp.Get<int>("@ResultVal"),
                ResultType = dp.Get<string>("@ResultType"),
                ResultMessage = dp.Get<string>("@ResultMessage")
            };

            var extraValue = dp.Get<string>($"@{extraOutputParamName}");

            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Ok);

            LogStoredProcedureExecuted(logger, tenant, FormatSpExec(storedProcedure, parameters), sw.Elapsed.TotalMilliseconds, spResult.ResultVal, spResult.ResultType);

            return (spResult, extraValue);
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            LogDatabaseError(logger, tenant, "StoredProcedure", FormatSpExec(storedProcedure, parameters), sw.Elapsed.TotalMilliseconds, ex);

            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "db.operation", "stored_procedure" },
                { "db.name", storedProcedure },
                { "tenant", tenant }
            };
            DbQueryDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            DbQueryCounter.Add(1, tags);
        }
    }

    /// <inheritdoc />
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var tenant = CurrentTenant;

        using var activity = DbActivity.StartActivity("DB QuerySingle");
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.operation.name", "QuerySingleOrDefault");
        activity?.SetTag("db.query.text", sql);
        activity?.SetTag("tenant", tenant);

        var sw = Stopwatch.StartNew();

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken);

            var result = await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);

            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Ok);

            LogQueryExecuted(logger, tenant, "QuerySingleOrDefault", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            LogDatabaseError(logger, tenant, "QuerySingleOrDefault", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds, ex);

            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "db.operation", "query_single" },
                { "db.name", sql },
                { "tenant", tenant }
            };
            DbQueryDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            DbQueryCounter.Add(1, tags);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var tenant = CurrentTenant;

        using var activity = DbActivity.StartActivity("DB Query");
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.operation.name", "Query");
        activity?.SetTag("db.query.text", sql);
        activity?.SetTag("tenant", tenant);

        var sw = Stopwatch.StartNew();

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken);

            var results = await connection.QueryAsync<T>(command).ConfigureAwait(false);

            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Ok);

            LogQueryExecuted(logger, tenant, "Query", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds);

            return results.AsList();
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            LogDatabaseError(logger, tenant, "Query", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds, ex);

            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "db.operation", "query" },
                { "db.name", sql },
                { "tenant", tenant }
            };
            DbQueryDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            DbQueryCounter.Add(1, tags);
        }
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var tenant = CurrentTenant;

        using var activity = DbActivity.StartActivity("DB Execute");
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.operation.name", "Execute");
        activity?.SetTag("db.query.text", sql);
        activity?.SetTag("tenant", tenant);

        var sw = Stopwatch.StartNew();

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken);

            var rowsAffected = await connection.ExecuteAsync(command).ConfigureAwait(false);

            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Ok);

            LogQueryExecuted(logger, tenant, "Execute", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds);

            return rowsAffected;
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            LogDatabaseError(logger, tenant, "Execute", InlineParameters(sql, parameters), sw.Elapsed.TotalMilliseconds, ex);

            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "db.operation", "execute" },
                { "db.name", sql },
                { "tenant", tenant }
            };
            DbQueryDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            DbQueryCounter.Add(1, tags);
        }
    }

    // -----------------------------------------------------------------------
    // Source-generated log messages
    // -----------------------------------------------------------------------

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "SQL StoredProcedure | {TenantId} | {ElapsedMs:F1}ms | ResultVal: {ResultVal} | ResultType: {ResultType}\n{ExecStatement}")]
    private static partial void LogStoredProcedureExecuted(
        ILogger logger, string tenantId, string execStatement, double elapsedMs, int resultVal, string resultType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "SQL {Operation} | {TenantId} | {ElapsedMs:F1}ms\n{Sql}")]
    private static partial void LogQueryExecuted(
        ILogger logger, string tenantId, string operation, string sql, double elapsedMs);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "SQL {Operation} failed | {TenantId} | {ElapsedMs:F1}ms\n{Sql}")]
    private static partial void LogDatabaseError(
        ILogger logger, string tenantId, string operation, string sql, double elapsedMs, Exception ex);

    // -----------------------------------------------------------------------
    // SSMS-ready SQL formatting helpers
    // -----------------------------------------------------------------------

    private static string InlineParameters(string sql, object? parameters)
    {
        if (parameters is null)
        {
            return sql;
        }

        foreach (var prop in parameters.GetType().GetProperties())
        {
            sql = sql.Replace($"@{prop.Name}", ToSqlLiteral(prop.GetValue(parameters)), StringComparison.OrdinalIgnoreCase);
        }

        return sql;
    }

    private static string FormatSpExec(string storedProcedure, Dictionary<string, object?> parameters)
    {
        if (parameters.Count == 0)
        {
            return $"EXEC {storedProcedure};";
        }

        var sb = new StringBuilder();
        sb.Append("EXEC ").AppendLine(storedProcedure);

        var index = 0;
        foreach (var (key, value) in parameters)
        {
            sb.Append("    @").Append(key).Append(" = ").Append(ToSqlLiteral(value));
            if (++index < parameters.Count)
            {
                sb.AppendLine(",");
            }
        }

        sb.Append(';');
        return sb.ToString();
    }

    private static string ToSqlLiteral(object? value) => value switch
    {
        null => "NULL",
        string s => $"N'{s.Replace("'", "''", StringComparison.Ordinal)}'",
        bool b => b ? "1" : "0",
        DateTime dt => FormattableString.Invariant($"'{dt:yyyy-MM-dd HH:mm:ss}'"),
        DateTimeOffset dto => FormattableString.Invariant($"'{dto:yyyy-MM-dd HH:mm:ss}'"),
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "NULL"
    };
}
