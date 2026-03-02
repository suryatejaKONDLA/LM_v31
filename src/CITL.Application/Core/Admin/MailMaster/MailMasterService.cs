using System.Globalization;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.MailMaster;

/// <summary>
/// Application service for Mail Master CRUD operations.
/// Caches GET results in Redis (L2) + MemoryCache (L1) with tenant-scoped keys.
/// Write operations invalidate all related cache entries.
/// </summary>
public sealed partial class MailMasterService(
    IMailMasterRepository repository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ITenantContext tenantContext,
    IValidator<MailMasterRequest> validator,
    ILogger<MailMasterService> logger) : IMailMasterService
{
    // ────────────────────────────────────────────────────────────────
    //  Cache key helpers — pattern: cache:{tenantId}:mail:{segment}
    // ────────────────────────────────────────────────────────────────

    private string CacheKeyAll(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:mail:all:{isApproved}");

    private string CacheKeyDropDown(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:mail:dropdown:{isApproved}");

    private string CacheKeyById(int mailSNo) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:mail:{mailSNo}");

    // ────────────────────────────────────────────────────────────────
    //  GET — cache-aside via GetOrSetAsync / GetAsync + SetAsync
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MailMasterResponse>>> GetAllAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var items = await cacheService.GetOrSetAsync(
            CacheKeyAll(isApproved),
            ct => repository.GetAllAsync(isApproved, ct),
            CacheEntryOptions.Default,
            cancellationToken).ConfigureAwait(false);

        return Result.Success(items);
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
    public async Task<Result<MailMasterResponse>> GetByIdAsync(int mailSNo, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<MailMasterResponse>(
            CacheKeyById(mailSNo), cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var item = await repository.GetByIdAsync(mailSNo, cancellationToken).ConfigureAwait(false);

        if (item is null)
        {
            return Result.Failure<MailMasterResponse>(Error.NotFound("MailMaster", mailSNo.ToString(CultureInfo.InvariantCulture)));
        }

        await cacheService.SetAsync(
            CacheKeyById(mailSNo), item, CacheEntryOptions.Default, cancellationToken).ConfigureAwait(false);

        return Result.Success(item);
    }

    // ────────────────────────────────────────────────────────────────
    //  WRITE — mutate DB then invalidate all related cache entries
    // ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Result> AddOrUpdateAsync(
        MailMasterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
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
            LogAddOrUpdateSucceeded(logger, request.MailFromAddress, spResult.ResultVal);
            await InvalidateMailCacheAsync(request.MailSNo, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToResult("MailMaster.SaveFailed");
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(int mailSNo, CancellationToken cancellationToken)
    {
        var spResult = await repository.DeleteAsync(mailSNo, cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogDeleteFailed(logger, mailSNo, spResult.ResultMessage);
        }
        else
        {
            LogDeleteSucceeded(logger, mailSNo);
            await InvalidateMailCacheAsync(mailSNo, cancellationToken).ConfigureAwait(false);
        }

        return spResult.ToResult("MailMaster.DeleteFailed");
    }

    // ────────────────────────────────────────────────────────────────
    //  Cache invalidation — removes all list/dropdown + specific item
    // ────────────────────────────────────────────────────────────────

    private async Task InvalidateMailCacheAsync(int mailSNo, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            cacheService.RemoveAsync(CacheKeyAll(true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyAll(false), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(true), cancellationToken),
            cacheService.RemoveAsync(CacheKeyDropDown(false), cancellationToken)).ConfigureAwait(false);

        if (mailSNo > 0)
        {
            await cacheService.RemoveAsync(CacheKeyById(mailSNo), cancellationToken).ConfigureAwait(false);
        }

        LogCacheInvalidated(logger, tenantContext.TenantId, mailSNo);
    }

    // ────────────────────────────────────────────────────────────────
    //  Source-generated log messages
    // ────────────────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Mail config add/update failed — Type: {ResultType}, Message: {ResultMessage}")]
    private static partial void LogAddOrUpdateFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Mail config '{MailFromAddress}' saved — ResultVal: {ResultVal}")]
    private static partial void LogAddOrUpdateSucceeded(ILogger logger, string mailFromAddress, int resultVal);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Mail config delete failed for MailSNo {MailSNo}: {Reason}")]
    private static partial void LogDeleteFailed(ILogger logger, int mailSNo, string reason);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Mail config deleted — MailSNo: {MailSNo}")]
    private static partial void LogDeleteSucceeded(ILogger logger, int mailSNo);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Mail cache invalidated — TenantId: {TenantId}, MailSNo: {MailSNo}")]
    private static partial void LogCacheInvalidated(ILogger logger, string tenantId, int mailSNo);
}
