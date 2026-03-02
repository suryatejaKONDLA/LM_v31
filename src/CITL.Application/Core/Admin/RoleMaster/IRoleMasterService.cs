using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.RoleMaster;

/// <summary>
/// Application service interface for Role Master CRUD operations.
/// </summary>
public interface IRoleMasterService
{
    /// <summary>Gets all roles, optionally filtered to approved-only.</summary>
    Task<Result<IReadOnlyList<RoleResponse>>> GetAllAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a simplified dropdown list of roles (ID + Name).</summary>
    Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a single role by ID.</summary>
    Task<Result<RoleResponse>> GetByIdAsync(int roleId, CancellationToken cancellationToken);

    /// <summary>Creates or updates a role.</summary>
    Task<Result> AddOrUpdateAsync(RoleMasterRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a role by ID.</summary>
    Task<Result> DeleteAsync(int roleId, CancellationToken cancellationToken);
}
