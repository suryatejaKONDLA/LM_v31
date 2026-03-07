using CITL.Application.Core.Admin.FinYearMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Admin;

/// <summary>
/// Manages citl.FIN_Year — CRUD for financial years.
/// </summary>
[Route("[controller]")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Administration)]
public sealed class FinYearMasterController(IFinYearMasterService finYearMasterService) : CitlControllerBase
{
    /// <summary>
    /// Gets all financial years.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FinYearResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await finYearMasterService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets a financial year by year code.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FinYearResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await finYearMasterService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Creates or updates a financial year.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateAsync(
        [FromBody] FinYearMasterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await finYearMasterService.AddOrUpdateAsync(request, cancellationToken);
        return result.IsSuccess
            ? FromResult(Result.Success(), result.Value)
            : FromResult(Result.Failure(result.Error));
    }

    /// <summary>
    /// Deletes a financial year.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await finYearMasterService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? FromResult(Result.Success(), result.Value)
            : FromResult(Result.Failure(result.Error));
    }
}
