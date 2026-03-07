using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.FinYearMaster;

/// <summary>
/// Repository contract for citl.FIN_Year.
/// </summary>
public interface IFinYearMasterRepository
{
    Task<IReadOnlyList<FinYearResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<FinYearResponse?> GetByIdAsync(int finYear, CancellationToken cancellationToken);
    Task<SpResult> AddOrUpdateAsync(FinYearMasterRequest request, CancellationToken cancellationToken);
    Task<SpResult> DeleteAsync(int finYear, CancellationToken cancellationToken);
}
