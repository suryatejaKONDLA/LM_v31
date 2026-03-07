using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Common.GenderMaster;

/// <summary>
/// Application service for Gender Master lookup operations.
/// </summary>
public sealed class GenderMasterService(IGenderMasterRepository repository) : IGenderMasterService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DropDownResponse<string>>>> GetDropDownAsync(
        CancellationToken cancellationToken)
    {
        var items = await repository.GetDropDownAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(items);
    }
}
