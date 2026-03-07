using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Common.GenderMaster;

namespace CITL.Infrastructure.Core.Common;

/// <inheritdoc />
internal sealed class GenderMasterRepository(IDbExecutor db) : IGenderMasterRepository
{
    private const string DropDownSql = """
        SELECT GENDER_Code AS Col1, GENDER_Name AS Col2
        FROM citl_sys.GENDER_Master
        ORDER BY GENDER_Order
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropDownResponse<string>>> GetDropDownAsync(
        CancellationToken cancellationToken)
    {
        return await db.QueryAsync<DropDownResponse<string>>(
            DropDownSql, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
