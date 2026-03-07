using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.Mappings.Mapping;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin.Mappings;

/// <summary>
/// Manages generic anchor-to-item mappings (Login-Role, Login-Branch, etc.).
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class MappingsController(IMappingsService mappingsService) : CitlControllerBase
{
    /// <summary>
    /// Gets existing mappings for the given anchor ID and mapping type.
    /// </summary>
    /// <param name="queryString">Mapping type code (e.g. 010703 = Login-Role)</param>
    /// <param name="anchorId">The anchor entity ID (e.g. Login_ID)</param>
    /// <param name="swapFlag">0 = normal direction, 1 = swapped direction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MappingsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByQueryStringAsync(
        [FromQuery] string? queryString,
        [FromQuery] string? anchorId,
        [FromQuery] int swapFlag,
        CancellationToken cancellationToken)
    {
        var result = await mappingsService.GetByQueryStringAsync(
            queryString ?? string.Empty,
            anchorId ?? string.Empty,
            swapFlag,
            cancellationToken);

        return FromResult(result);
    }

    // TODO: Remove this endpoint once Login Master feature is implemented — dropdown should be served from there.
    /// <summary>
    /// Returns a dropdown list of all login users.
    /// </summary>
    [HttpGet("LoginDropDown")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DropDownResponse<int>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginDropDownAsync(CancellationToken cancellationToken)
    {
        var result = await mappingsService.GetLoginDropDownAsync(cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Inserts or replaces mappings for the given anchor in a bulk operation.
    /// </summary>
    /// <param name="request">Mapping request with query string, anchor ID, and list of item IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InsertAsync(
        [FromBody] MappingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mappingsService.InsertAsync(request, cancellationToken);
        return FromResult(result);
    }
}
