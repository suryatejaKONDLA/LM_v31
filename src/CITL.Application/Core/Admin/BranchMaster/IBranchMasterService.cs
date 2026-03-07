using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.BranchMaster;

/// <summary>
/// Application service interface for Branch Master CRUD operations.
/// </summary>
public interface IBranchMasterService
{
    /// <summary>Gets all branches with optional active/approved filters.</summary>
    Task<Result<IReadOnlyList<BranchResponse>>> GetAllAsync(bool isActive, bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a simplified dropdown list of branches (Code + Name).</summary>
    Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a single branch by code.</summary>
    Task<Result<BranchResponse>> GetByIdAsync(int branchCode, CancellationToken cancellationToken);

    /// <summary>Creates or updates a branch.</summary>
    Task<Result<string>> AddOrUpdateAsync(BranchMasterRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a branch by code.</summary>
    Task<Result<string>> DeleteAsync(int branchCode, CancellationToken cancellationToken);
}
