using CITL.Application.Core.Admin.Mappings.RoleMenuMapping;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin.Mappings;

/// <summary>
/// Manages Role-to-Menu access mappings.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class RoleMenuMappingController(IRoleMenuMappingService roleMenuMappingService) : CitlControllerBase
{
    /// <summary>
    /// Gets all menu mappings configured for the specified role.
    /// </summary>
    /// <param name="roleId">The role identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of Role-Menu mappings</returns>
    [HttpGet("{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleMenuMappingResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        var result = await roleMenuMappingService.GetByRoleIdAsync(roleId, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Inserts or replaces all menu mappings for a role in a bulk operation.
    /// </summary>
    /// <param name="request">The mapping DTO containing the RoleId and a list of MenuIds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result message indicating success or failure from the database</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] RoleMenuMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await roleMenuMappingService.AddOrUpdateAsync(request, cancellationToken);
        return FromResult(result);
    }
}
