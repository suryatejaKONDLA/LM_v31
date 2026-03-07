using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.Mappings.Mapping;

/// <summary>
/// Data-access contract for the generic mapping feature.
/// </summary>
public interface IMappingsRepository
{
    /// <summary>Fetches existing mappings for the given anchor ID and mapping type.</summary>
    Task<IReadOnlyList<MappingsResponse>> GetByQueryStringAsync(
        string queryString,
        string anchorId,
        int swapFlag,
        CancellationToken cancellationToken);

    /// <summary>Returns a login dropdown list (ID + Name).</summary>
    Task<IReadOnlyList<DropDownResponse<int>>> GetLoginDropDownAsync(CancellationToken cancellationToken);

    /// <summary>Inserts or replaces mappings via stored procedure.</summary>
    Task<SpResult> InsertAsync(MappingsRequest request, int sessionId, CancellationToken cancellationToken);
}
