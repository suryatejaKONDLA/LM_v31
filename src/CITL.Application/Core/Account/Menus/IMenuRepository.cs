namespace CITL.Application.Core.Account.Menus;

/// <summary>
/// Repository interface for menu data access via <c>citltvf.Login_Menus</c>.
/// Defined in Application; implemented in Infrastructure with Dapper.
/// </summary>
public interface IMenuRepository
{
    /// <summary>
    /// Returns a flat list of all menu items accessible to the specified login,
    /// ordered by <c>MENU_ID</c>.
    /// </summary>
    /// <param name="loginId">The login identifier passed to <c>citltvf.Login_Menus</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A flat, ordered list of menu items with empty <c>Children</c> collections.</returns>
    Task<IReadOnlyList<MenuResponse>> GetMenusAsync(int loginId, CancellationToken cancellationToken);
}
