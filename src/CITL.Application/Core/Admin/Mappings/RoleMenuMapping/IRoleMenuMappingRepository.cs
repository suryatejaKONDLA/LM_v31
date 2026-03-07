using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.Mappings.RoleMenuMapping;

public interface IRoleMenuMappingRepository
{
    Task<IReadOnlyList<RoleMenuMappingResponse>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken);

    Task<SpResult> AddOrUpdateAsync(RoleMenuMappingRequest request, int sessionId, CancellationToken cancellationToken);
}
