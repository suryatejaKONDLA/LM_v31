using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.MailMaster;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages Mail_Master — CRUD operations for mail (SMTP) configurations.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class MailMasterController(IMailMasterService mailMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets all mail configurations, optionally filtered to approved-only.
    /// </summary>
    /// <param name="isApproved">When <c>true</c> (default), returns only approved configurations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of mail configurations.</returns>
    /// <response code="200">Returns all mail configurations.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MailMasterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mailMasterService.GetAllAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a simplified dropdown list of mail configurations (SNo + FromAddress) for UI select lists.
    /// </summary>
    /// <param name="isApproved">When <c>true</c> (default), returns only approved configurations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of mail SNo/FromAddress pairs.</returns>
    /// <response code="200">Returns the dropdown list.</response>
    [HttpGet("DropDown")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DropDownResponse<int>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDropDownAsync(
        [FromQuery] bool isApproved = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mailMasterService.GetDropDownAsync(isApproved, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a single mail configuration by serial number.
    /// </summary>
    /// <param name="id">The mail serial number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mail configuration details.</returns>
    /// <response code="200">Returns the mail configuration.</response>
    /// <response code="404">Mail configuration not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MailMasterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mailMasterService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates a mail configuration. Send Mail_SNo = 0 for insert, or an existing SNo for update.
    /// </summary>
    /// <param name="request">The mail configuration data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Mail configuration saved successfully.</response>
    /// <response code="400">Validation failed or SP returned an error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] MailMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mailMasterService.AddOrUpdateAsync(request, cancellationToken);
        return FromResult(result, "Mail configuration saved successfully.");
    }

    /// <summary>
    /// Deletes a mail configuration by serial number.
    /// Also removes related activity mail log records.
    /// </summary>
    /// <param name="id">The mail serial number to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Mail configuration deleted successfully.</response>
    /// <response code="400">Mail configuration cannot be deleted.</response>
    /// <response code="404">Mail configuration not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mailMasterService.DeleteAsync(id, cancellationToken);
        return FromResult(result, "Mail configuration deleted successfully.");
    }
}
