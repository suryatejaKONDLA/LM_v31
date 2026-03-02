using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// Application service interface for App Master operations.
/// Orchestrates validation, repository calls, and result mapping.
/// </summary>
public interface IAppMasterService
{
    /// <summary>
    /// Adds or updates the application master configuration.
    /// </summary>
    /// <param name="request">The add/update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the SP message on success, or an error.</returns>
    Task<Result> AddOrUpdateAsync(AppMasterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the application master configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the response data, or an error if not found.</returns>
    Task<Result<AppMasterResponse>> GetAsync(CancellationToken cancellationToken);
}
