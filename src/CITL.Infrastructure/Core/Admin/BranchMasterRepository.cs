using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.BranchMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <inheritdoc />
internal sealed class BranchMasterRepository(IDbExecutor db) : IBranchMasterRepository
{
    private const string GetAllBaseSql = """
        SELECT
            T1.*, T2.*, T3.*,
            cu.Login_Name AS BRANCH_Created_Name,
            mu.Login_Name AS BRANCH_Modified_Name,
            au.Login_Name AS BRANCH_Approved_Name
        FROM citl.BRANCH_Master T1
            INNER JOIN citl.BRANCH_Master_Extend T2 ON T2.BRANCH_Code = T1.BRANCH_Code
            LEFT  JOIN citl.BRANCH_Master_Logo   T3 ON T3.BRANCH_Code = T1.BRANCH_Code
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = T2.BRANCH_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = T2.BRANCH_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = T2.BRANCH_Approved_ID
        WHERE T1.BRANCH_Code > 0
        """;

    private const string GetByIdSql = """
        SELECT
            T1.*, T2.*, T3.*,
            cu.Login_Name AS BRANCH_Created_Name,
            mu.Login_Name AS BRANCH_Modified_Name,
            au.Login_Name AS BRANCH_Approved_Name
        FROM citl.BRANCH_Master T1
            INNER JOIN citl.BRANCH_Master_Extend T2 ON T2.BRANCH_Code = T1.BRANCH_Code
            LEFT  JOIN citl.BRANCH_Master_Logo   T3 ON T3.BRANCH_Code = T1.BRANCH_Code
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = T2.BRANCH_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = T2.BRANCH_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = T2.BRANCH_Approved_ID
        WHERE T1.BRANCH_Code = @BranchCode
        """;

    private const string DropDownBaseSql = """
        SELECT T1.BRANCH_Code AS Col1, T1.BRANCH_Name AS Col2
        FROM citl.BRANCH_Master T1
            INNER JOIN citl.BRANCH_Master_Extend T2 ON T2.BRANCH_Code = T1.BRANCH_Code
        WHERE T1.BRANCH_Code > 0
        """;

    private const string DeleteSql = """
        DELETE FROM citl.BRANCH_Master_Logo   WHERE BRANCH_Code = @BranchCode;
        DELETE FROM citl.BRANCH_Master_Extend WHERE BRANCH_Code = @BranchCode;
        DELETE FROM citl.BRANCH_Master        WHERE BRANCH_Code = @BranchCode;

        IF @@ROWCOUNT = 0
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Branch not found.' AS ResultMessage;
        ELSE
            SELECT 1 AS ResultVal, 'success' AS ResultType, 'Branch deleted successfully.' AS ResultMessage;
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<BranchResponse>> GetAllAsync(
        bool isActive, bool isApproved, CancellationToken cancellationToken)
    {
        var sql = GetAllBaseSql;

        if (isActive)
        {
            sql += " AND T2.BRANCH_Active_Flag = 1";
        }

        if (isApproved)
        {
            sql += " AND T2.BRANCH_Approved_ID IS NOT NULL";
        }

        sql += " ORDER BY T2.BRANCH_Order";

        return await db.QueryAsync<BranchResponse>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(
        bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{DropDownBaseSql} AND T2.BRANCH_Approved_ID IS NOT NULL ORDER BY T1.BRANCH_Name"
            : $"{DropDownBaseSql} ORDER BY T1.BRANCH_Name";

        return await db.QueryAsync<DropDownResponse<int>>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BranchResponse?> GetByIdAsync(int branchCode, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<BranchResponse>(
            GetByIdSql,
            new { BranchCode = branchCode },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(
        BranchMasterRequest request,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["BRANCH_Code"] = request.BranchCode,
            ["BRANCH_Name"] = request.BranchName,
            ["BRANCH_State"] = request.BranchState,
            ["BRANCH_Name2"] = request.BranchName2,
            ["BRANCH_Address1"] = request.BranchAddress1,
            ["BRANCH_Address2"] = request.BranchAddress2,
            ["BRANCH_Address3"] = request.BranchAddress3,
            ["BRANCH_City"] = request.BranchCity,
            ["BRANCH_PIN"] = request.BranchPin,
            ["BRANCH_Contact_Person"] = request.BranchContactPerson,
            ["BRANCH_Phone_No1"] = request.BranchPhoneNo1,
            ["BRANCH_Phone_No2"] = request.BranchPhoneNo2,
            ["BRANCH_Email_ID"] = request.BranchEmailId,
            ["BRANCH_GSTIN"] = request.BranchGstin,
            ["BRANCH_PAN_No"] = request.BranchPanNo,
            ["BRANCH_AutoApproval_Enabled"] = request.BranchAutoApprovalEnabled,
            ["BRANCH_Discounts_Enabled"] = request.BranchDiscountsEnabled,
            ["BRANCH_CreditLimits_Enabled"] = request.BranchCreditLimitsEnabled,
            ["BRANCH_Currency_Code"] = request.BranchCurrencyCode,
            ["BRANCH_TimeZone_Code"] = request.BranchTimeZoneCode,
            ["BRANCH_Order"] = request.BranchOrder,
            ["BRANCH_Active_Flag"] = request.BranchActiveFlag,
            ["BRANCH_Logo"] = request.BranchLogo,
            ["Session_ID"] = sessionId
        };

        return await db.ExecuteSpAsync("citlsp.BRANCH_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> DeleteAsync(int branchCode, CancellationToken cancellationToken)
    {
        var result = await db.QuerySingleOrDefaultAsync<SpResult>(
            DeleteSql,
            new { BranchCode = branchCode },
            cancellationToken).ConfigureAwait(false);

        return result ?? new SpResult { ResultVal = -1, ResultType = "error", ResultMessage = "Branch not found." };
    }
}
