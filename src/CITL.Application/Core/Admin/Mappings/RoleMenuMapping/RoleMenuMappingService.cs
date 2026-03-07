using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.Mappings.RoleMenuMapping;

public sealed partial class RoleMenuMappingService(
    IRoleMenuMappingRepository repository,
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    IValidator<RoleMenuMappingRequest> validator,
    ILogger<RoleMenuMappingService> logger) : IRoleMenuMappingService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Getting role menu mappings for Role ID: {RoleId} (Tenant: {TenantId})")]
    private partial void LogGettingRoleMenuMappings(int roleId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating role menu mappings for Role ID: {RoleId} (Tenant: {TenantId})")]
    private partial void LogUpdatingRoleMenuMappings(int roleId, string tenantId);

    public async Task<Result<IReadOnlyList<RoleMenuMappingResponse>>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        LogGettingRoleMenuMappings(roleId, tenantContext.TenantId);

        var mappings = await repository.GetByRoleIdAsync(roleId, cancellationToken).ConfigureAwait(false);
        return Result.Success(mappings);
    }

    public async Task<Result<string>> AddOrUpdateAsync(RoleMenuMappingRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return Result.Failure<string>(Error.Validation("Validation", validationResult.Errors[0].ErrorMessage));
        }

        LogUpdatingRoleMenuMappings(request.RoleId, tenantContext.TenantId);

        var result = await repository.AddOrUpdateAsync(request, currentUser.LoginId, cancellationToken).ConfigureAwait(false);

        return result.ToMessageResult("RoleMenuMapping.SaveFailed");
    }
}
