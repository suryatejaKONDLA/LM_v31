using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.RoleMaster;

/// <summary>
/// Repository interface for Role Master database operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IRoleMasterRepository
{
    /// <summary>
    /// Gets all roles, optionally filtered to approved-only.
    /// </summary>
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a simplified dropdown list of roles (ID + Name), optionally filtered to approved-only.
    /// </summary>
    Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single role by ID.
    /// </summary>
    Task<RoleResponse?> GetByIdAsync(int roleId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a role by calling <c>citlsp.ROLE_Insert</c>.
    /// When <c>request.RoleId</c> is 0, the SP auto-generates the ID.
    /// </summary>
    Task<SpResult> AddOrUpdateAsync(RoleMasterRequest request, int sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a role by ID. Fails if the role is referenced by Login_ROLE_Mapping or ROLE_MENU_Mapping.
    /// </summary>
    Task<SpResult> DeleteAsync(int roleId, CancellationToken cancellationToken);
}
