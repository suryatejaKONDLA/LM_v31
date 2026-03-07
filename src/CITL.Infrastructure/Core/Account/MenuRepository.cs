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
        SELECT *
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

    public async Task<IReadOnlyList<MenuResponse>> GetAllMenusAsync(CancellationToken cancellationToken)
    {
        const string GetAllMenusSql = """
            SELECT *
            FROM citltvf.Login_Menus(1)
            ORDER BY MENU_ID
            """;

        return await db.QueryAsync<MenuResponse>(GetAllMenusSql, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
