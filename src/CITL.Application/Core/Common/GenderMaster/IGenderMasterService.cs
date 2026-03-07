using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Common.GenderMaster;

/// <summary>
/// Application service interface for Gender Master lookup operations.
/// </summary>
public interface IGenderMasterService
{
    /// <summary>Gets gender dropdown values from <c>citl_sys.GENDER_Master</c>.</summary>
    Task<Result<IReadOnlyList<DropDownResponse<string>>>> GetDropDownAsync(CancellationToken cancellationToken);
}
