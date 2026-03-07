using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.FinYearMaster;

/// <summary>
/// Service contract for FIN Year Master.
/// </summary>
public interface IFinYearMasterService
{
    Task<Result<IReadOnlyList<FinYearResponse>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<FinYearResponse>> GetByIdAsync(int finYear, CancellationToken cancellationToken);
    Task<Result<string>> AddOrUpdateAsync(FinYearMasterRequest request, CancellationToken cancellationToken);
    Task<Result<string>> DeleteAsync(int finYear, CancellationToken cancellationToken);
}
