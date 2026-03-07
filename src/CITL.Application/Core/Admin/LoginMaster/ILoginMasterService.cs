using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.LoginMaster;

/// <summary>
/// Application service interface for Login Master operations.
/// </summary>
public interface ILoginMasterService
{
    /// <summary>Gets a single login by ID.</summary>
    Task<Result<LoginMasterResponse>> GetByIdAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>Gets a dropdown list of logins.</summary>
    Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Creates or updates a login. Sends a welcome email on new insert.</summary>
    Task<Result<string>> AddOrUpdateAsync(LoginMasterRequest request, CancellationToken cancellationToken);
}
