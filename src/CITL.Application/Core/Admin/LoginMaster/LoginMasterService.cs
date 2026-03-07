using System.Globalization;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.Application.Core.Admin.AppMaster;
using CITL.Application.Core.Admin.BranchMaster;
using CITL.Application.Core.Notifications.Email;
using CITL.Application.Core.Notifications.Email.Templates;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.LoginMaster;

/// <summary>
/// Application service for Login Master operations.
/// Sends a welcome email in the background when a new login is created.
/// </summary>
public sealed partial class LoginMasterService(
    ILoginMasterRepository repository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    ITenantContext tenantContext,
    IBackgroundEmailDispatcher emailDispatcher,
    IAppMasterService appMasterService,
    IBranchMasterService branchMasterService,
    IValidator<LoginMasterRequest> validator,
    ILogger<LoginMasterService> logger) : ILoginMasterService
{
    private string CacheKeyById(int loginId) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:login:{loginId}");

    private string CacheKeyDropDown(bool isApproved) =>
        string.Create(CultureInfo.InvariantCulture, $"{AuthConstants.CacheKeyPrefix}:{tenantContext.TenantId}:login:dropdown:{isApproved}");

    /// <inheritdoc />
    public async Task<Result<LoginMasterResponse>> GetByIdAsync(
        int loginId, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<LoginMasterResponse>(
            CacheKeyById(loginId), cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var login = await repository.GetByIdAsync(loginId, cancellationToken).ConfigureAwait(false);

        if (login is null)
        {
            return Result.Failure<LoginMasterResponse>(Error.NotFound("Login.NotFound", "Login not found."));
        }

        await cacheService.SetAsync(
            CacheKeyById(loginId), login, CacheEntryOptions.Default, cancellationToken).ConfigureAwait(false);

        return Result.Success(login);
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
    public async Task<Result<string>> AddOrUpdateAsync(
        LoginMasterRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult<string>();
        }

        var isInsert = request.LoginId == 0;
        LoginInsertSpResult spResult;

        if (isInsert)
        {
            spResult = await repository.InsertAsync(
                request,
                currentUser.LoginId,
                request.BranchCode,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            spResult = await repository.UpdateAsync(
                request,
                currentUser.LoginId,
                request.BranchCode,
                cancellationToken).ConfigureAwait(false);
        }

        if (!spResult.IsSuccess)
        {
            LogLoginSaveFailed(logger, spResult.ResultType, spResult.ResultMessage);
            return Result.Failure<string>(new Error("Login.SaveFailed", spResult.ResultMessage));
        }

        LogLoginSaveSucceeded(logger, request.LoginUser, spResult.ResultVal);

        // Invalidate cache
        if (!isInsert)
        {
            await cacheService.RemoveAsync(CacheKeyById(request.LoginId), cancellationToken).ConfigureAwait(false);
        }

        await cacheService.RemoveAsync(CacheKeyDropDown(true), cancellationToken).ConfigureAwait(false);
        await cacheService.RemoveAsync(CacheKeyDropDown(false), cancellationToken).ConfigureAwait(false);

        // Welcome email — data fetched within request scope, SMTP dispatched in background
        if (isInsert && !string.IsNullOrWhiteSpace(spResult.ReturnPassword) && !string.IsNullOrWhiteSpace(request.LoginEmailId))
        {
            await EnqueueWelcomeEmailAsync(request, spResult.ReturnPassword, cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(spResult.ResultMessage);
    }

    // ── Welcome email ──────────────────────────────────────────────────────────

    private async Task EnqueueWelcomeEmailAsync(
        LoginMasterRequest request,
        string password,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch data while scoped services are still alive (within the HTTP request)
            var appResult = await appMasterService.GetAsync(cancellationToken).ConfigureAwait(false);
            var branchResult = await branchMasterService.GetByIdAsync(
                request.BranchCode, cancellationToken).ConfigureAwait(false);

            var appName = appResult.IsSuccess ? appResult.Value.AppHeader1 : "CITL";
            var appLogo = appResult.IsSuccess ? appResult.Value.AppLogo1 : null;
            var supportEmail = branchResult.IsSuccess ? branchResult.Value.BranchEmailId : string.Empty;
            var supportPhone = branchResult.IsSuccess ? branchResult.Value.BranchPhoneNo1 : string.Empty;

            InlineImage? logoImage = null;

            if (appLogo is { Length: > 0 })
            {
                logoImage = new InlineImage
                {
                    ContentId = "app-logo",
                    MimeType = "image/png",
                    Content = appLogo,
                };
            }

            var body = EmailTemplates.BuildWelcomeEmail(
                appName,
                logoImage is not null,
                request.LoginName,
                request.LoginUser,
                password,
                supportEmail,
                supportPhone);

            // Only the SMTP send runs in background via dispatcher
            emailDispatcher.Enqueue(
                tenantContext.TenantId,
                tenantContext.DatabaseName,
                request.LoginEmailId,
                $"Welcome to {appName} — Your Account Details",
                body,
                logoImage is not null ? [logoImage] : null);
        }
        catch (Exception ex)
        {
            LogWelcomeEmailFailed(logger, ex, request.LoginEmailId);
        }
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Login save failed — ResultType: '{ResultType}', Message: '{ResultMessage}'")]
    private static partial void LogLoginSaveFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Login saved — LoginUser: '{LoginUser}', LoginId: {LoginId}")]
    private static partial void LogLoginSaveSucceeded(ILogger logger, string loginUser, int loginId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Welcome email failed to enqueue — Recipient: '{Email}'")]
    private static partial void LogWelcomeEmailFailed(ILogger logger, Exception ex, string email);
}
