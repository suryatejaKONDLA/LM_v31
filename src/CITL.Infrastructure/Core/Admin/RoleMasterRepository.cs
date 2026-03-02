using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.RoleMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <inheritdoc />
internal sealed class RoleMasterRepository(IDbExecutor db) : IRoleMasterRepository
{
    private const string GetAllBaseSql = """
        SELECT
            rm.ROLE_ID, rm.ROLE_Name, rm.ROLE_Branch_Code,
            rm.ROLE_Created_ID,  cu.Login_Name AS RoleCreatedName,  rm.ROLE_Created_Date,
            rm.ROLE_Modified_ID, mu.Login_Name AS RoleModifiedName, rm.ROLE_Modified_Date,
            rm.ROLE_Approved_ID, au.Login_Name AS RoleApprovedName, rm.ROLE_Approved_Date
        FROM citl.ROLE_Master rm
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = rm.ROLE_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = rm.ROLE_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = rm.ROLE_Approved_ID
        """;

    private const string GetByIdSql = """
        SELECT
            rm.ROLE_ID, rm.ROLE_Name, rm.ROLE_Branch_Code,
            rm.ROLE_Created_ID,  cu.Login_Name AS RoleCreatedName,  rm.ROLE_Created_Date,
            rm.ROLE_Modified_ID, mu.Login_Name AS RoleModifiedName, rm.ROLE_Modified_Date,
            rm.ROLE_Approved_ID, au.Login_Name AS RoleApprovedName, rm.ROLE_Approved_Date
        FROM citl.ROLE_Master rm
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = rm.ROLE_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = rm.ROLE_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = rm.ROLE_Approved_ID
        WHERE rm.ROLE_ID = @RoleId
        """;

    private const string DropDownBaseSql = """
        SELECT ROLE_ID AS Col1, ROLE_Name AS Col2
        FROM citl.ROLE_Master
        """;

    private const string DeleteSql = """
        IF EXISTS (SELECT 1 FROM citl.Login_ROLE_Mapping WHERE ROLE_ID = @RoleId)
        BEGIN
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Cannot delete: role is assigned to one or more users.' AS ResultMessage;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM citl.ROLE_MENU_Mapping WHERE ROLE_ID = @RoleId)
        BEGIN
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Cannot delete: role has menu permissions assigned.' AS ResultMessage;
            RETURN;
        END

        DELETE FROM citl.ROLE_Master WHERE ROLE_ID = @RoleId;

        IF @@ROWCOUNT = 0
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Role not found.' AS ResultMessage;
        ELSE
            SELECT 1 AS ResultVal, 'success' AS ResultType, 'Role deleted successfully.' AS ResultMessage;
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{GetAllBaseSql} WHERE rm.ROLE_Approved_ID IS NOT NULL ORDER BY rm.ROLE_Name"
            : $"{GetAllBaseSql} ORDER BY rm.ROLE_Name";

        return await db.QueryAsync<RoleResponse>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{DropDownBaseSql} WHERE ROLE_Approved_ID IS NOT NULL ORDER BY ROLE_Name"
            : $"{DropDownBaseSql} ORDER BY ROLE_Name";

        return await db.QueryAsync<DropDownResponse<int>>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<RoleResponse?> GetByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<RoleResponse>(
            GetByIdSql,
            new { RoleId = roleId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(
        RoleMasterRequest request,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["ROLE_ID"] = request.RoleId,
            ["ROLE_Name"] = request.RoleName,
            ["Branch_Code"] = request.BranchCode,
            ["Session_ID"] = sessionId
        };

        return await db.ExecuteSpAsync("citlsp.ROLE_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> DeleteAsync(int roleId, CancellationToken cancellationToken)
    {
        var result = await db.QuerySingleOrDefaultAsync<SpResult>(
            DeleteSql,
            new { RoleId = roleId },
            cancellationToken).ConfigureAwait(false);

        return result ?? new SpResult { ResultVal = -1, ResultType = "error", ResultMessage = "Role not found." };
    }
}
