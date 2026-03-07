using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.Mappings.Mapping;

/// <summary>
/// Business-logic contract for the generic mapping feature.
/// </summary>
public interface IMappingsService
{
    /// <summary>Gets existing mappings for the given anchor and mapping type.</summary>
    Task<Result<IReadOnlyList<MappingsResponse>>> GetByQueryStringAsync(
        string queryString,
        string anchorId,
        int swapFlag,
        CancellationToken cancellationToken);

    /// <summary>Returns a dropdown list of logins.</summary>
    Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetLoginDropDownAsync(CancellationToken cancellationToken);

    /// <summary>Inserts or replaces mappings.</summary>
    Task<Result<string>> InsertAsync(MappingsRequest request, CancellationToken cancellationToken);
}
