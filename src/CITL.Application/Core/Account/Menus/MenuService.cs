using CITL.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Account.Menus;

/// <summary>
/// Application service for menu retrieval and optional tree building.
/// </summary>
/// <remarks>
/// Reads a flat ordered list from <c>citltvf.Login_Menus</c> and, when <c>asTree=true</c>,
/// constructs the parent–child hierarchy in memory using each item's <c>MenuParentId</c>.
/// </remarks>
/// <param name="repository">The menu repository.</param>
/// <param name="logger">The logger.</param>
public sealed partial class MenuService(
    IMenuRepository repository,
    ILogger<MenuService> logger) : IMenuService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MenuResponse>>> GetMenusAsync(
        int loginId,
        bool asTree,
        CancellationToken cancellationToken)
    {
        var menus = await repository.GetMenusAsync(loginId, cancellationToken).ConfigureAwait(false);

        if (menus.Count == 0)
        {
            LogMenusNotFound(logger, loginId);

            return Result.Failure<IReadOnlyList<MenuResponse>>(
                Error.NotFound(nameof(MenuResponse), $"No menus found for login {loginId}."));
        }

        var result = asTree ? BuildTree(menus) : menus;

        LogMenusRetrieved(logger, loginId, menus.Count, asTree);

        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<MenuResponse>>> GetAllMenusAsync(bool asTree, CancellationToken cancellationToken)
    {
        var menus = await repository.GetAllMenusAsync(cancellationToken).ConfigureAwait(false);

        if (menus.Count == 0)
        {
            return Result.Failure<IReadOnlyList<MenuResponse>>(
                Error.NotFound(nameof(MenuResponse), "No master menus found."));
        }

        var result = asTree ? BuildTree(menus) : menus;

        return Result.Success(result);
    }

    /// <summary>
    /// Builds a parent-child tree from a flat, <c>MENU_ID</c>-ordered list.
    /// Items with no matching parent are treated as root nodes.
    /// </summary>
    private static List<MenuResponse> BuildTree(IReadOnlyList<MenuResponse> flat)
    {
        var lookup = new Dictionary<string, MenuResponse>(flat.Count, StringComparer.Ordinal);

        foreach (var item in flat)
        {
            lookup[item.MenuId] = item;
        }

        var roots = new List<MenuResponse>();

        foreach (var item in flat)
        {
            if (string.IsNullOrEmpty(item.MenuParentId) ||
                !lookup.TryGetValue(item.MenuParentId, out var parent))
            {
                roots.Add(item);
            }
            else
            {
                parent.Children.Add(item);
            }
        }

        return roots;
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No menus found for LoginId: {LoginId}")]
    private static partial void LogMenusNotFound(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Menus retrieved — LoginId: {LoginId}, Count: {Count}, AsTree: {AsTree}")]
    private static partial void LogMenusRetrieved(ILogger logger, int loginId, int count, bool asTree);
}
