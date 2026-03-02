using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.MailMaster;

/// <summary>
/// Application service interface for Mail Master CRUD operations.
/// </summary>
public interface IMailMasterService
{
    /// <summary>Gets all mail configurations, optionally filtered to approved-only.</summary>
    Task<Result<IReadOnlyList<MailMasterResponse>>> GetAllAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a simplified dropdown list of mail configurations (SNo + FromAddress).</summary>
    Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>Gets a single mail configuration by serial number.</summary>
    Task<Result<MailMasterResponse>> GetByIdAsync(int mailSNo, CancellationToken cancellationToken);

    /// <summary>Creates or updates a mail configuration.</summary>
    Task<Result> AddOrUpdateAsync(MailMasterRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a mail configuration by serial number.</summary>
    Task<Result> DeleteAsync(int mailSNo, CancellationToken cancellationToken);
}
