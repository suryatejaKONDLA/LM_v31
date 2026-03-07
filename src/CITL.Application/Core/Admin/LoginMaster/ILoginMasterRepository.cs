using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.LoginMaster;

/// <summary>
/// Repository interface for Login Master data access.
/// </summary>
public interface ILoginMasterRepository
{
    /// <summary>Gets a single login with audit trail by ID.</summary>
    Task<LoginMasterResponse?> GetByIdAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>Gets a dropdown list of logins.</summary>
    Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Executes <c>citlsp.Login_Insert</c> and returns the SP result including the auto-generated password.</summary>
    Task<LoginInsertSpResult> InsertAsync(LoginMasterRequest request, int sessionId, int branchCode, CancellationToken cancellationToken);

    /// <summary>Executes <c>citlsp.Login_Insert</c> in update mode (Login_ID > 0).</summary>
    Task<LoginInsertSpResult> UpdateAsync(LoginMasterRequest request, int sessionId, int branchCode, CancellationToken cancellationToken);
}
