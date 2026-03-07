using System.Data;
using System.Globalization;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.Mappings.RoleMenuMapping;
using Dapper;

namespace CITL.Infrastructure.Core.Admin.Mappings;

internal sealed class RoleMenuMappingRepository(IDbExecutor db) : IRoleMenuMappingRepository
{
    public async Task<IReadOnlyList<RoleMenuMappingResponse>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT T1.*,
                   T2.Login_Name AS CreatedByName,
                   T3.Login_Name AS ModifiedByName,
                   T4.Login_Name AS ApprovedByName
            FROM citl.ROLE_MENU_Mapping T1
            INNER JOIN citl.Login_Name T2 ON T1.ROLE_MENU_Created_ID = T2.Login_ID
            LEFT JOIN citl.Login_Name T3 ON T1.ROLE_MENU_Modified_ID = T3.Login_ID
            LEFT JOIN citl.Login_Name T4 ON T1.ROLE_MENU_Approved_ID = T4.Login_ID
            WHERE T1.ROLE_ID = @RoleId
            ORDER BY T1.MENU_ID;
            """;
        return await db.QueryAsync<RoleMenuMappingResponse>(sql, new { RoleId = roleId }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task<SpResult> AddOrUpdateAsync(RoleMenuMappingRequest request, int sessionId, CancellationToken cancellationToken)
    {
        return db.ExecuteSpAsync("citlsp.ROLE_MENU_Mapping_Insert", new Dictionary<string, object?>
        {
            { "Mapping_TBType1", CreateTableValuedParameter(request).AsTableValuedParameter("citltypes.Mapping_TBType1") },
            { "Session_ID", sessionId }
        }, cancellationToken: cancellationToken);
    }

    private static DataTable CreateTableValuedParameter(RoleMenuMappingRequest request)
    {
        var dt = new DataTable();
        dt.Columns.Add("Left_Column", typeof(string));
        dt.Columns.Add("Right_Column", typeof(string));

        var roleIdStr = request.RoleId.ToString(CultureInfo.InvariantCulture);

        if (request.MenuIds.Count > 0)
        {
            foreach (var menuId in request.MenuIds)
            {
                dt.Rows.Add(roleIdStr, menuId);
            }
        }
        else
        {
            dt.Rows.Add(roleIdStr, DBNull.Value);
        }

        return dt;
    }
}
