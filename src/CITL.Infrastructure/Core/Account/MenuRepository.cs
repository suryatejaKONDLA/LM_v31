using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Account.Menus;

namespace CITL.Infrastructure.Core.Account;

/// <summary>
/// Dapper implementation of <see cref="IMenuRepository"/>.
/// Queries the table-valued function <c>citltvf.Login_Menus</c>.
/// </summary>
/// <param name="db">The tenant-scoped database executor.</param>
internal sealed class MenuRepository(IDbExecutor db) : IMenuRepository
{
    private const string GetMenusSql = """
        SELECT
            MENU_ID, MENU_Name, MENU_Description, MENU_Parent_ID,
            MENU_URL1, MENU_URL2, MENU_URL3, MENU_Flag,
            MENU_Icon1, MENU_Icon2, MENU_Startup_Flag
        FROM citltvf.Login_Menus(@LoginId)
        ORDER BY MENU_ID
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<MenuResponse>> GetMenusAsync(
        int loginId, CancellationToken cancellationToken)
    {
        var parameters = new { LoginId = loginId };

        return await db.QueryAsync<MenuResponse>(GetMenusSql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }
}
