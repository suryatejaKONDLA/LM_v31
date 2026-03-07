using System.Globalization;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.BranchMaster;

/// <summary>
/// Application service for Branch Master CRUD operations.
/// Caches GET results with tenant-scoped keys. Write operations invalidate cache.
/// </summary>
public sealed partial class BranchMasterService(
    IBranchMasterRepository repository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ITenantContext tenantContext,
    IValidator<BranchMasterRequest> validator,
    ILogger<BranchMasterService> logger) : IBranchMasterService
{
    // ────────────────────────────────────────────────────────────────
    //  Cache key helpers — pattern: cache:{tenantId}:branch:{segment}
    // ────────────────────────────────────────────────────────────────

    private string CacheKeyAll(bool isActive, bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:branch:all:{isActive}:{isApproved}");

    private string CacheKeyDropDown(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:branch:dropdown:{isApproved}");

    private string CacheKeyById(int branchCode) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:branch:{branchCode}");

    // ────────────────────────────────────────────────────────────────
    //  GET — cache-aside
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<BranchResponse>>> GetAllAsync(
        bool isActive, bool isApproved, CancellationToken cancellationToken)
    {
        var branches = await cacheService.GetOrSetAsync(
            CacheKeyAll(isActive, isApproved),
            ct => repository.GetAllAsync(isActive, isApproved, ct),
            CacheEntryOptions.Default,
            cancellationToken).ConfigureAwait(false);

        return Result.Success(branches);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetDropDownAsync(
        bool isApproved, CancellationToken cancellationToken)
    {
        var items = await cacheService.GetOrSetAsync(
            CacheKeyDropDown(isApproved),
            ct => repository.GetDropDownAsync(isApproved, ct),
            CacheEntryOptions.Default,
            cancellationToken).ConfigureAwait(false);

        return Result.Success(items);
    }

    /// <inheritdoc />
    public async Task<Result<BranchResponse>> GetByIdAsync(int branchCode, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<BranchResponse>(
            CacheKeyById(branchCode), cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var branch = await repository.GetByIdAsync(branchCode, cancellationToken).ConfigureAwait(false);

        if (branch is null)
        {
            return Result.Failure<BranchResponse>(Error.NotFound("Branch.NotFound", "Branch not found."));
        }

        await cacheService.SetAsync(
            CacheKeyById(branchCode), branch, CacheEntryOptions.Default, cancellationToken).ConfigureAwait(false);

        return Result.Success(branch);
    }

    // ────────────────────────────────────────────────────────────────
    //  WRITE — mutate DB then invalidate cache
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<string>> AddOrUpdateAsync(
        BranchMasterRequest request,
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
            LogAddOrUpdateSucceeded(logger, request.BranchName, spResult.ResultVal);
            await InvalidateBranchCacheAsync(request.BranchCode, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToMessageResult("Branch.SaveFailed");
    }

    /// <inheritdoc />
    public async Task<Result<string>> DeleteAsync(int branchCode, CancellationToken cancellationToken)
    {
        var spResult = await repository.DeleteAsync(branchCode, cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogDeleteFailed(logger, branchCode, spResult.ResultMessage);
        }
        else
        {
            LogDeleteSucceeded(logger, branchCode);
            await InvalidateBranchCacheAsync(branchCode, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToMessageResult("Branch.DeleteFailed");
    }

    // ────────────────────────────────────────────────────────────────
    //  Private helpers
    // ────────────────────────────────────────────────────────────────

    private async Task InvalidateBranchCacheAsync(int branchCode, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            cacheService.RemoveAsync(CacheKeyAll(true, true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyAll(true, false), cancellationToken),
            cacheService.RemoveAsync(CacheKeyAll(false, true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyAll(false, false), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(false), cancellationToken)).ConfigureAwait(false);

        if (branchCode > 0)
        {
            await cacheService.RemoveAsync(CacheKeyById(branchCode), cancellationToken).ConfigureAwait(false);
        }
    }

    // ────────────────────────────────────────────────────────────────
    //  LoggerMessage source generators
    // ────────────────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Warning, Message = "Branch AddOrUpdate failed: {ResultType} — {ResultMessage}")]
    private static partial void LogAddOrUpdateFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Branch '{BranchName}' saved (ResultVal={ResultVal})")]
    private static partial void LogAddOrUpdateSucceeded(ILogger logger, string branchName, int resultVal);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Branch delete failed for code={BranchCode}: {ResultMessage}")]
    private static partial void LogDeleteFailed(ILogger logger, int branchCode, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Branch code={BranchCode} deleted")]
    private static partial void LogDeleteSucceeded(ILogger logger, int branchCode);
}
