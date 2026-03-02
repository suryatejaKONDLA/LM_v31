using CITL.Application.Core.Admin.AppMaster;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages the application master configuration (branding, logos).
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class AppMasterController(IAppMasterService appMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets the application master configuration with audit trail.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint — requires <c>X-Tenant-Id</c> header but no JWT.
    /// Returns the application branding info including logos and audit user names.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application master data, or 404 if not configured.</returns>
    /// <response code="200">Returns the application master configuration.</response>
    /// <response code="404">Application configuration not found.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AppMasterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await appMasterService.GetAsync(cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates the application master configuration.
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
        [FromBody] AppMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await appMasterService.AddOrUpdateAsync(request, cancellationToken);
        return FromResult(result, "Application configuration saved successfully.");
    }
}
