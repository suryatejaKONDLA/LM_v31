using CITL.Application.Core.Admin.CompanyMaster;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages the company master configuration (name, contact, logos).
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class CompanyMasterController(ICompanyMasterService companyMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets the company master configuration with audit trail.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint — requires <c>X-Tenant-Id</c> header but no JWT.
    /// Returns the company info including logos and audit user names.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The company master data, or 404 if not configured.</returns>
    /// <response code="200">Returns the company master configuration.</response>
    /// <response code="404">Company configuration not found.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CompanyMasterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await companyMasterService.GetAsync(cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates the company master configuration.
    /// </summary>
    /// <remarks>
    /// Accepts JSON body with optional base64-encoded logo images.
    /// </remarks>
    /// <param name="request">The add/update request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>200 OK on success, or 400/404 with error details.</returns>
    /// <response code="200">Configuration saved successfully.</response>
    /// <response code="400">Validation failed or stored procedure returned an error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] CompanyMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await companyMasterService.AddOrUpdateAsync(request, cancellationToken);
        return FromResult(result, "Company configuration saved successfully.");
    }
}
