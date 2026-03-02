using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// Application service interface for Company Master operations.
/// Orchestrates validation, repository calls, and result mapping.
/// </summary>
public interface ICompanyMasterService
{
    /// <summary>
    /// Adds or updates the company master configuration.
    /// </summary>
    /// <param name="request">The add/update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the SP message on success, or an error.</returns>
    Task<Result> AddOrUpdateAsync(CompanyMasterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the company master configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the response data, or an error if not found.</returns>
    Task<Result<CompanyMasterResponse>> GetAsync(CancellationToken cancellationToken);
}
