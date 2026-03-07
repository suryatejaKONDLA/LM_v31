using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Admin.Mappings.RoleMenuMapping;

public interface IRoleMenuMappingService
{
    Task<Result<IReadOnlyList<RoleMenuMappingResponse>>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken);

    Task<Result<string>> AddOrUpdateAsync(RoleMenuMappingRequest request, CancellationToken cancellationToken);
}
