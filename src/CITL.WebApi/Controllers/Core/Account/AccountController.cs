using CITL.Application.Core.Account;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Account;

/// <summary>
/// Manages the authenticated user's account: profile retrieval, profile update, and password change.
/// </summary>
[Route("Account")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Account)]
public sealed class AccountController(IAccountService accountService) : CitlControllerBase
{
    /// <summary>
    /// Gets the current user's profile including personal info and profile picture (base64).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user profile.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet("Profile")]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
    {
        var result = await accountService.GetProfileAsync(cancellationToken);
        return FromResult(result, "Profile retrieved successfully.");
    }

    /// <summary>
    /// Updates the current user's profile (name, contact info, profile picture, startup page).
    /// </summary>
    /// <param name="request">The profile update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPut("Profile")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.UpdateProfileAsync(request, cancellationToken);
        return FromResult(result, "Profile updated successfully.");
    }

    /// <summary>
    /// Changes the current user's password. Validates the old password server-side.
    /// </summary>
    /// <param name="request">The change password request containing old and new passwords.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Validation failed or incorrect current password.</response>
    [HttpPut("Password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await accountService.ChangePasswordAsync(request, cancellationToken);
        return FromResult(result, "Password changed successfully.");
    }
}
