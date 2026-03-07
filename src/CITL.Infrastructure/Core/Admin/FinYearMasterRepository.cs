using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.FinYearMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <inheritdoc />
internal sealed class FinYearMasterRepository(IDbExecutor db) : IFinYearMasterRepository
{
    private const string GetAllSql = """
        SELECT FIN_Year, FIN_Date1, FIN_Date2, FIN_Active_Flag
        FROM citl.FIN_Year
        ORDER BY FIN_Year DESC
        """;

    private const string GetByIdSql = """
        SELECT FIN_Year, FIN_Date1, FIN_Date2, FIN_Active_Flag
        FROM citl.FIN_Year
        WHERE FIN_Year = @FinYear
        """;

    private const string DeleteSql = """
        DELETE FROM citl.FIN_Year WHERE FIN_Year = @FinYear;

        IF @@ROWCOUNT = 0
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Financial year not found.' AS ResultMessage;
        ELSE
            SELECT 1 AS ResultVal, 'success' AS ResultType, 'Financial year deleted successfully.' AS ResultMessage;
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<FinYearResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.QueryAsync<FinYearResponse>(GetAllSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<FinYearResponse?> GetByIdAsync(int finYear, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<FinYearResponse>(
            GetByIdSql, new { FinYear = finYear }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(FinYearMasterRequest request, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["FIN_Year"] = request.FinYear,
            ["FIN_Date1"] = request.FinDate1,
            ["FIN_Date2"] = request.FinDate2,
            ["FIN_Active_Flag"] = request.FinActiveFlag
        };

        return await db.ExecuteSpAsync("citlsp.FIN_Year_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> DeleteAsync(int finYear, CancellationToken cancellationToken)
    {
        var result = await db.QuerySingleOrDefaultAsync<SpResult>(
            DeleteSql, new { FinYear = finYear }, cancellationToken).ConfigureAwait(false);

        return result ?? new SpResult { ResultVal = -1, ResultType = "error", ResultMessage = "Financial year not found." };
    }
}
