using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.BranchMaster;

/// <summary>
/// Repository interface for Branch Master database operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IBranchMasterRepository
{
    /// <summary>
    /// Gets all branches with optional active/approved filters.
    /// </summary>
    Task<IReadOnlyList<BranchResponse>> GetAllAsync(bool isActive, bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a simplified dropdown list of branches (Code + Name).
    /// </summary>
    Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single branch by code.
    /// </summary>
    Task<BranchResponse?> GetByIdAsync(int branchCode, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a branch by calling <c>citlsp.BRANCH_Insert</c>.
    /// </summary>
    Task<SpResult> AddOrUpdateAsync(BranchMasterRequest request, int sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a branch and its related extend/logo records.
    /// </summary>
    Task<SpResult> DeleteAsync(int branchCode, CancellationToken cancellationToken);
}
