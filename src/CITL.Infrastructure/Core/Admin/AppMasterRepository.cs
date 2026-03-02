using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.AppMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <summary>
/// Dapper implementation of <see cref="IAppMasterRepository"/>.
/// Queries <c>citl.App_Master</c> and calls <c>citlsp.App_Insert</c>.
/// </summary>
/// <param name="db">The tenant-scoped database executor.</param>
internal sealed class AppMasterRepository(IDbExecutor db) : IAppMasterRepository
{
    private const string GetSql = """
        SELECT
            am.APP_Code, am.APP_Header1, am.APP_Header2, am.APP_Link,
            am.APP_Logo1, am.APP_Logo2, am.APP_Logo3,
            am.APP_Created_ID, cu.Login_Name AS AppCreatedName, am.APP_Created_Date,
            am.APP_Modified_ID, mu.Login_Name AS AppModifiedName, am.APP_Modified_Date,
            am.APP_Approved_ID, au.Login_Name AS AppApprovedName, am.APP_Approved_Date
        FROM citl.App_Master am
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = am.APP_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = am.APP_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = am.APP_Approved_ID
        """;

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(
        AppMasterRequest request, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["APP_Code"] = request.AppCode,
            ["APP_Header1"] = request.AppHeader1,
            ["APP_Header2"] = request.AppHeader2,
            ["APP_Link"] = request.AppLink,
            ["APP_Logo1"] = request.AppLogo1,
            ["APP_Logo2"] = request.AppLogo2,
            ["APP_Logo3"] = request.AppLogo3,
            ["Session_ID"] = request.SessionId,
            ["BRANCH_Code"] = request.BranchCode
        };

        return await db.ExecuteSpAsync("citlsp.App_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AppMasterResponse?> GetAsync(CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<AppMasterResponse>(GetSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
