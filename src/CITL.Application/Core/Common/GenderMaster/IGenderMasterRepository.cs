using CITL.Application.Common.Models;

namespace CITL.Application.Core.Common.GenderMaster;

/// <summary>
/// Repository interface for Gender Master lookup data.
/// Data is sourced from <c>citl_sys.GENDER_Master</c> — shared across all tenants.
/// </summary>
public interface IGenderMasterRepository
{
    /// <summary>Returns all active genders ordered by display order.</summary>
    Task<IReadOnlyList<DropDownResponse<string>>> GetDropDownAsync(CancellationToken cancellationToken);
}
