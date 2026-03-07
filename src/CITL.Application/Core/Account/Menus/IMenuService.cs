using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Account.Menus;

/// <summary>
/// Service interface for menu retrieval use cases.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Gets the menus for the specified login as either a flat list or a parent-child tree.
    /// </summary>
    /// <param name="loginId">The login identifier passed to <c>citltvf.Login_Menus</c>.</param>
    /// <param name="asTree">
    /// When <see langword="true"/>, returns menus in a hierarchical tree (root items contain children).
    /// When <see langword="false"/>, returns a flat list with empty <c>Children</c> collections.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The menu list, or a <c>NotFound</c> failure result if no menus exist for the login.</returns>
    Task<Result<IReadOnlyList<MenuResponse>>> GetMenusAsync(
        int loginId,
        bool asTree,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<MenuResponse>>> GetAllMenusAsync(bool asTree, CancellationToken cancellationToken);
}
