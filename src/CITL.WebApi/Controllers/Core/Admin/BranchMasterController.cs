using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.BranchMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages BRANCH_Master — CRUD operations for branches.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class BranchMasterController(IBranchMasterService branchMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets all branches with optional active/approved filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BranchResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] bool isActive = true,
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await branchMasterService.GetAllAsync(isActive, isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a simplified dropdown list of branches (Code + Name).
    /// </summary>
    [HttpGet("DropDown")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DropDownResponse<int>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDropDownAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await branchMasterService.GetDropDownAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a single branch by code.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await branchMasterService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates a branch. Logo bytes are sent as base64 on the request body.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] BranchMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await branchMasterService.AddOrUpdateAsync(request, cancellationToken);
        return result.IsSuccess
            ? FromResult(Result.Success(), result.Value)
            : FromResult(Result.Failure(result.Error));
    }

    /// <summary>
    /// Deletes a branch by code.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await branchMasterService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? FromResult(Result.Success(), result.Value)
            : FromResult(Result.Failure(result.Error));
    }
}
