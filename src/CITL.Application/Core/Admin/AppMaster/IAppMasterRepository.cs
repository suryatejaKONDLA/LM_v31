using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// Repository interface for <c>citl.App_Master</c> operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IAppMasterRepository
{
    /// <summary>
    /// Inserts or updates the application master configuration
    /// by calling stored procedure <c>citlsp.App_Insert</c>.
    /// </summary>
    /// <param name="request">The add/update request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored procedure result containing status and message.</returns>
    Task<SpResult> AddOrUpdateAsync(AppMasterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the application master configuration with audit user names
    /// joined from <c>citl.Login_Name</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application master response, or <see langword="null"/> if not configured.</returns>
    Task<AppMasterResponse?> GetAsync(CancellationToken cancellationToken);
}
