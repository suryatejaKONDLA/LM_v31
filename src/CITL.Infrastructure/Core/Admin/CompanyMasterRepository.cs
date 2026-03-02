using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.CompanyMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <summary>
/// Dapper implementation of <see cref="ICompanyMasterRepository"/>.
/// Queries <c>citl.CMP_Company</c> and calls <c>citlsp.Company_Insert</c>.
/// </summary>
/// <param name="db">The tenant-scoped database executor.</param>
internal sealed class CompanyMasterRepository(IDbExecutor db) : ICompanyMasterRepository
{
    private const string GetSql = """
        SELECT
            cm.CMP_Code, cm.CMP_Full_Name, cm.CMP_Short_Name,
            cm.CMP_Mobile1, cm.CMP_Mobile2, cm.CMP_Email,
            cm.CMP_Website, cm.CMP_Tagline,
            cm.CMP_Logo1, cm.CMP_Logo2, cm.CMP_Logo3,
            cm.CMP_Created_ID, cu.Login_Name AS CmpCreatedName, cm.CMP_Created_Date,
            cm.CMP_Modified_ID, mu.Login_Name AS CmpModifiedName, cm.CMP_Modified_Date,
            cm.CMP_Approved_ID, au.Login_Name AS CmpApprovedName, cm.CMP_Approved_Date
        FROM citl.CMP_Company cm
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = cm.CMP_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = cm.CMP_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = cm.CMP_Approved_ID
        """;

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(
        CompanyMasterRequest request, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["CMP_Code"] = request.CompanyCode,
            ["CMP_Full_Name"] = request.FullName,
            ["CMP_Short_Name"] = request.ShortName,
            ["CMP_Mobile1"] = request.Mobile1,
            ["CMP_Mobile2"] = request.Mobile2,
            ["CMP_Email"] = request.Email,
            ["CMP_Website"] = request.Website,
            ["CMP_Tagline"] = request.Tagline,
            ["CMP_Logo1"] = request.Logo1,
            ["CMP_Logo2"] = request.Logo2,
            ["CMP_Logo3"] = request.Logo3,
            ["Session_ID"] = request.SessionId,
            ["BRANCH_Code"] = request.BranchCode
        };

        return await db.ExecuteSpAsync("citlsp.Company_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CompanyMasterResponse?> GetAsync(CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<CompanyMasterResponse>(GetSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
