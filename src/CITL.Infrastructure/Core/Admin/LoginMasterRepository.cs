using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.LoginMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <inheritdoc />
internal sealed class LoginMasterRepository(IDbExecutor db) : ILoginMasterRepository
{
    private const string GetByIdSql = """
        SELECT
            T1.Login_ID, T1.Login_User, T1.Login_Name, T1.Login_Branch_Code,
            T2.Login_Designation, T2.Login_Mobile_No, T2.Login_Email_ID,
            T2.Login_DOB, T2.Login_Gender, T2.Login_Active_Flag,
            T2.Login_Created_ID, cu.Login_Name AS Login_Created_Name, T2.Login_Created_Date,
            T2.Login_Modified_ID, mu.Login_Name AS Login_Modified_Name, T2.Login_Modified_Date,
            T2.Login_Approved_ID, au.Login_Name AS Login_Approved_Name, T2.Login_Approved_Date
        FROM citl.Login_Master T1
            INNER JOIN citl.Login_Master2 T2 ON T2.Login_ID = T1.Login_ID
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = T2.Login_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = T2.Login_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = T2.Login_Approved_ID
        WHERE T1.Login_ID = @LoginId
        """;

    private const string DropDownBaseSql = """
        SELECT T1.Login_ID AS Col1, T1.Login_Name AS Col2
        FROM citl.Login_Master T1
            INNER JOIN citl.Login_Master2 T2 ON T2.Login_ID = T1.Login_ID
        WHERE T1.Login_ID > 0
        """;

    /// <inheritdoc />
    public async Task<LoginMasterResponse?> GetByIdAsync(
        int loginId, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<LoginMasterResponse>(
            GetByIdSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(
        bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{DropDownBaseSql} AND T2.Login_Approved_ID IS NOT NULL ORDER BY T1.Login_Name"
            : $"{DropDownBaseSql} ORDER BY T1.Login_Name";

        return await db.QueryAsync<DropDownResponse<int>>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<LoginInsertSpResult> InsertAsync(
        LoginMasterRequest request,
        int sessionId,
        int branchCode,
        CancellationToken cancellationToken)
    {
        var parameters = BuildSpParameters(request, 0, sessionId, branchCode);
        var (spResult, returnPassword) = await db.ExecuteSpAsync(
            "citlsp.Login_Insert", parameters, "ReturnPassword", cancellationToken).ConfigureAwait(false);

        return new LoginInsertSpResult
        {
            ResultVal = spResult.ResultVal,
            ResultType = spResult.ResultType,
            ResultMessage = spResult.ResultMessage,
            ReturnPassword = returnPassword,
        };
    }

    /// <inheritdoc />
    public async Task<LoginInsertSpResult> UpdateAsync(
        LoginMasterRequest request,
        int sessionId,
        int branchCode,
        CancellationToken cancellationToken)
    {
        var parameters = BuildSpParameters(request, request.LoginId, sessionId, branchCode);
        var (spResult, returnPassword) = await db.ExecuteSpAsync(
            "citlsp.Login_Insert", parameters, "ReturnPassword", cancellationToken).ConfigureAwait(false);

        return new LoginInsertSpResult
        {
            ResultVal = spResult.ResultVal,
            ResultType = spResult.ResultType,
            ResultMessage = spResult.ResultMessage,
            ReturnPassword = returnPassword,
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Dictionary<string, object?> BuildSpParameters(
        LoginMasterRequest request,
        int loginId,
        int sessionId,
        int branchCode)
    {
        return new Dictionary<string, object?>
        {
            ["Login_ID"] = loginId,
            ["Login_User"] = request.LoginUser,
            ["Login_Name"] = request.LoginName,
            ["Login_Designation"] = request.LoginDesignation,
            ["Login_Mobile_No"] = request.LoginMobileNo,
            ["Login_Email_ID"] = request.LoginEmailId,
            ["Login_DOB"] = request.LoginDob.HasValue
                ? (object)request.LoginDob.Value.ToDateTime(TimeOnly.MinValue)
                : null,
            ["Login_Gender"] = request.LoginGender,
            ["Login_Active_Flag"] = request.LoginActiveFlag,
            ["Session_ID"] = sessionId,
            ["BRANCH_Code"] = branchCode,
        };
    }
}
