using CITL.Application.Core.Account.Theme;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Account;

/// <summary>
/// Manages the authenticated user's theme preferences.
/// </summary>
[Route("Account/Theme")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Account)]
public sealed class ThemeController(IThemeService themeService) : CitlControllerBase
{
    /// <summary>
    /// Gets the current user's theme configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's theme JSON.</returns>
    /// <response code="200">Theme retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ThemeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await themeService.GetAsync(cancellationToken);
        return FromResult(result, "Theme retrieved successfully.");
    }

    /// <summary>
    /// Saves the current user's theme configuration.
    /// </summary>
    /// <param name="request">The theme data to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Theme saved successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveAsync(
        [FromBody] SaveThemeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await themeService.SaveAsync(request, cancellationToken);
        return FromResult(result, "Theme saved successfully.");
    }
}
