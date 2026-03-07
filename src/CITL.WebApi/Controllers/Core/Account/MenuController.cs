using CITL.Application.Core.Account.Menus;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Account;

/// <summary>
/// Provides menu navigation items for a given login, with optional tree structuring.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Account)]
public sealed class MenuController(IMenuService menuService) : CitlControllerBase
{
    /// <summary>
    /// Gets the navigation menus for the specified login.
    /// </summary>
    /// <remarks>
    /// Set <paramref name="asTree"/> to <c>true</c> to receive a hierarchical tree where each
    /// root item contains its children. Set to <c>false</c> (default) for a flat list suitable
    /// for manual rendering or custom grouping.
    /// </remarks>
    /// <param name="loginId">The login identifier — maps to the <c>citltvf.Login_Menus</c> parameter.</param>
    /// <param name="asTree">
    /// When <c>true</c>, menus are returned as a parent–child tree.
    /// When <c>false</c> (default), a flat ordered list is returned with empty <c>children</c> arrays.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The menu list, or 404 if no menus are configured for the login.</returns>
    /// <response code="200">Returns the menu list (flat or tree).</response>
    /// <response code="404">No menus found for the specified login.</response>
    [HttpGet("{loginId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(
        int loginId,
        [FromQuery] bool asTree,
        CancellationToken cancellationToken)
    {
        var result = await menuService.GetMenusAsync(loginId, asTree, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets all active system menus.
    /// </summary>
    /// <param name="asTree">When true, returns a hierarchical tree. Otherwise, a flat list.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of master menus</returns>
    [HttpGet("All")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool asTree, CancellationToken cancellationToken)
    {
        var result = await menuService.GetAllMenusAsync(asTree, cancellationToken);
        return FromResult(result);
    }
}
