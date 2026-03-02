using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.RoleMaster;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages ROLE_Master — CRUD operations for user roles.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class RoleMasterController(IRoleMasterService roleMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets all roles, optionally filtered to approved-only.
    /// </summary>
    /// <param name="isApproved">When <c>true</c> (default), returns only approved roles.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of roles.</returns>
    /// <response code="200">Returns all roles.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await roleMasterService.GetAllAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a simplified dropdown list of roles (ID + Name) for UI select lists.
    /// </summary>
    /// <param name="isApproved">When <c>true</c> (default), returns only approved roles.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of role ID/Name pairs.</returns>
    /// <response code="200">Returns the dropdown list.</response>
    [HttpGet("DropDown")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DropDownResponse<int>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDropDownAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await roleMasterService.GetDropDownAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a single role by ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role details.</returns>
    /// <response code="200">Returns the role.</response>
    /// <response code="404">Role not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await roleMasterService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates a role. Send ROLE_ID = 0 for insert, or an existing ID for update.
    /// </summary>
    /// <param name="request">The role data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Role saved successfully.</response>
    /// <response code="400">Validation failed or SP returned an error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] RoleMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await roleMasterService.AddOrUpdateAsync(request, cancellationToken);
        return FromResult(result, "Role saved successfully.");
    }

    /// <summary>
    /// Deletes a role by ID. Fails if the role is assigned to users or has menu mappings.
    /// </summary>
    /// <param name="id">The role ID to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Role deleted successfully.</response>
    /// <response code="400">Role is in use and cannot be deleted.</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await roleMasterService.DeleteAsync(id, cancellationToken);
        return FromResult(result, "Role deleted successfully.");
    }
}
