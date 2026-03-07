using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.LoginMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages Login_Master — user account CRUD operations.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class LoginMasterController(ILoginMasterService loginMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets a login record by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LoginMasterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await loginMasterService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a simplified dropdown list of logins.
    /// </summary>
    [HttpGet("DropDown")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DropDownResponse<int>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDropDownAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await loginMasterService.GetDropDownAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates a login. On insert, sends a welcome email with credentials to the login's email address.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] LoginMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loginMasterService.AddOrUpdateAsync(request, cancellationToken);
        return result.IsSuccess
            ? FromResult(Result.Success(), result.Value)
            : FromResult(Result.Failure(result.Error));
    }
}
