using CITL.Application.Common.Models;
using CITL.Application.Core.Common.GenderMaster;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Common;

/// <summary>
/// Provides lookup data for <c>citl_sys.GENDER_Master</c>.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Common)]
public sealed class GenderMasterController(IGenderMasterService genderMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets gender dropdown values.
    /// </summary>
    [HttpGet("DropDown")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DropDownResponse<string>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDropDownAsync(CancellationToken cancellationToken)
    {
        var result = await genderMasterService.GetDropDownAsync(cancellationToken);
        return FromResult(result);
    }
}
