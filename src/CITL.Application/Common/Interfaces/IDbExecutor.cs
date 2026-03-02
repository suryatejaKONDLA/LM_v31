using CITL.Application.Common.Models;

namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Provides high-level, tenant-scoped database operations for repositories.
/// Handles connection lifecycle, parameter binding, and SP output parameter extraction
/// so repositories contain zero Dapper/ADO boilerplate.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <term>CUD operations</term>
///     <description>Use <see cref="ExecuteSpAsync"/> — automatically adds
///     <c>@ResultVal</c>, <c>@ResultType</c>, <c>@ResultMessage</c> output parameters
///     and returns <see cref="SpResult"/>.</description>
///   </item>
///   <item>
///     <term>Read single</term>
///     <description>Use <see cref="QuerySingleOrDefaultAsync{T}"/>.</description>
///   </item>
///   <item>
///     <term>Read list</term>
///     <description>Use <see cref="QueryAsync{T}"/>.</description>
///   </item>
/// </list>
/// Defined in Application, implemented in Infrastructure via Dapper.
/// </remarks>
public interface IDbExecutor
{
    /// <summary>
    /// Executes a stored procedure that follows the CITL output contract
    /// (<c>@ResultVal</c>, <c>@ResultType</c>, <c>@ResultMessage</c>).
    /// </summary>
    /// <param name="storedProcedure">The fully-qualified SP name (e.g., <c>citlsp.App_Insert</c>).</param>
    /// <param name="parameters">
    /// Input parameter dictionary — keys are SP parameter names <b>without</b> the <c>@</c> prefix.
    /// Values must be CLR types that Dapper can map (int, string, byte[], etc.).
    /// The three standard output parameters are added automatically.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="SpResult"/> populated from the SP output parameters.</returns>
    Task<SpResult> ExecuteSpAsync(
        string storedProcedure,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries a single row or returns <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The result type to map to.</typeparam>
    /// <param name="sql">The SQL query or SP name.</param>
    /// <param name="parameters">Optional anonymous object or <c>Dictionary</c> of parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped result, or <c>null</c> if no rows found.</returns>
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries multiple rows.
    /// </summary>
    /// <typeparam name="T">The result type to map each row to.</typeparam>
    /// <param name="sql">The SQL query or SP name.</param>
    /// <param name="parameters">Optional anonymous object or <c>Dictionary</c> of parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of mapped results.</returns>
    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a non-query SQL statement (INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <param name="parameters">Optional anonymous object or <c>Dictionary</c> of parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}
