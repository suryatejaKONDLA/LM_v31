using System.Globalization;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.RoleMaster;

/// <summary>
/// Application service for Role Master CRUD operations.
/// Caches GET results in Redis (L2) + MemoryCache (L1) with tenant-scoped keys.
/// Write operations invalidate all related cache entries.
/// </summary>
public sealed partial class RoleMasterService(
    IRoleMasterRepository repository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ITenantContext tenantContext,
    IValidator<RoleMasterRequest> validator,
    ILogger<RoleMasterService> logger) : IRoleMasterService
{
    // ────────────────────────────────────────────────────────────────
    //  Cache key helpers — pattern: cache:{tenantId}:role:{segment}
    // ────────────────────────────────────────────────────────────────

    private string CacheKeyAll(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:role:all:{isApproved}");

    private string CacheKeyDropDown(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:role:dropdown:{isApproved}");

    private string CacheKeyById(int roleId) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:role:{roleId}");

    // ────────────────────────────────────────────────────────────────
    //  GET — cache-aside via GetOrSetAsync / GetAsync + SetAsync
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RoleResponse>>> GetAllAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var roles = await cacheService.GetOrSetAsync(
            CacheKeyAll(isApproved),
            ct => repository.GetAllAsync(isApproved, ct),
            CacheEntryOptions.Default,
            cancellationToken).ConfigureAwait(false);

        return Result.Success(roles);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var items = await cacheService.GetOrSetAsync(
            CacheKeyDropDown(isApproved),
            ct => repository.GetDropDownAsync(isApproved, ct),
            CacheEntryOptions.Default,
            cancellationToken).ConfigureAwait(false);

        return Result.Success(items);
    }

    /// <inheritdoc />
    public async Task<Result<RoleResponse>> GetByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<RoleResponse>(
            CacheKeyById(roleId), cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var role = await repository.GetByIdAsync(roleId, cancellationToken).ConfigureAwait(false);

        if (role is null)
        {
            return Result.Failure<RoleResponse>(Error.NotFound("Role", roleId.ToString(CultureInfo.InvariantCulture)));
        }

        await cacheService.SetAsync(
            CacheKeyById(roleId), role, CacheEntryOptions.Default, cancellationToken).ConfigureAwait(false);

        return Result.Success(role);
    }

    // ────────────────────────────────────────────────────────────────
    //  WRITE — mutate DB then invalidate all related cache entries
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<string>> AddOrUpdateAsync(
        RoleMasterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult<string>();
        }

        var spResult = await repository.AddOrUpdateAsync(
            request,
            currentUser.LoginId,
            cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogAddOrUpdateFailed(logger, spResult.ResultType, spResult.ResultMessage);
        }
        else
        {
            LogAddOrUpdateSucceeded(logger, request.RoleName, spResult.ResultVal);
            await InvalidateRoleCacheAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToMessageResult("Role.SaveFailed");
    }

    /// <inheritdoc />
    public async Task<Result<string>> DeleteAsync(int roleId, CancellationToken cancellationToken)
    {
        var spResult = await repository.DeleteAsync(roleId, cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogDeleteFailed(logger, roleId, spResult.ResultMessage);
        }
        else
        {
            LogDeleteSucceeded(logger, roleId);
            await InvalidateRoleCacheAsync(roleId, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToMessageResult("Role.DeleteFailed");
    }

    // ────────────────────────────────────────────────────────────────
    //  Cache invalidation — removes all list/dropdown + specific role
    // ────────────────────────────────────────────────────────────────

    private async Task InvalidateRoleCacheAsync(int roleId, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            cacheService.RemoveAsync(CacheKeyAll(true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyAll(false), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(false), cancellationToken)).ConfigureAwait(false);

        if (roleId > 0)
        {
            await cacheService.RemoveAsync(CacheKeyById(roleId), cancellationToken).ConfigureAwait(false);
        }

        LogCacheInvalidated(logger, tenantContext.TenantId, roleId);
    }

    // ────────────────────────────────────────────────────────────────
    //  Source-generated log messages
    // ────────────────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Role add/update failed — Type: {ResultType}, Message: {ResultMessage}")]
    private static partial void LogAddOrUpdateFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Role '{RoleName}' saved — ResultVal: {ResultVal}")]
    private static partial void LogAddOrUpdateSucceeded(ILogger logger, string roleName, int resultVal);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Role delete failed for RoleId {RoleId}: {Reason}")]
    private static partial void LogDeleteFailed(ILogger logger, int roleId, string reason);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Role deleted — RoleId: {RoleId}")]
    private static partial void LogDeleteSucceeded(ILogger logger, int roleId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Role cache invalidated — TenantId: {TenantId}, RoleId: {RoleId}")]
    private static partial void LogCacheInvalidated(ILogger logger, string tenantId, int roleId);
}
