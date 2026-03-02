using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// Repository interface for <c>citl.CMP_Company</c> operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface ICompanyMasterRepository
{
    /// <summary>
    /// Inserts or updates the company master configuration
    /// by calling stored procedure <c>citlsp.Company_Insert</c>.
    /// </summary>
    /// <param name="request">The add/update request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored procedure result containing status and message.</returns>
    Task<SpResult> AddOrUpdateAsync(CompanyMasterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the company master configuration with audit user names
    /// joined from <c>citl.Login_Name</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The company master response, or <see langword="null"/> if not configured.</returns>
    Task<CompanyMasterResponse?> GetAsync(CancellationToken cancellationToken);
}
