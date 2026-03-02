using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Account;

/// <summary>
/// Application service for account management: profile and password operations.
/// Uses FluentValidation for input validation and <see cref="SpResultExtensions"/>
/// for SP result conversion.
/// </summary>
public sealed partial class AccountService(
    IAccountRepository accountRepository,
    ICurrentUser currentUser,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IValidator<UpdateProfileRequest> updateProfileValidator,
    ILogger<AccountService> logger) : IAccountService
{
    /// <inheritdoc />
    public async Task<Result<ProfileResponse>> GetProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await accountRepository.GetProfileAsync(
            currentUser.LoginId,
            cancellationToken).ConfigureAwait(false);

        if (profile is null)
        {
            LogProfileNotFound(logger, currentUser.LoginId);
            return Result.Failure<ProfileResponse>(
                Error.NotFound("Account.Profile", currentUser.LoginUser));
        }

        LogProfileRetrieved(logger, currentUser.LoginId);
        return Result.Success(profile);
    }

    /// <inheritdoc />
    public async Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await changePasswordValidator
            .ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var spResult = await accountRepository.ChangePasswordAsync(
            currentUser.LoginId,
            request.LoginPassword,
            request.LoginPasswordOld,
            cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogPasswordChangeFailed(logger, currentUser.LoginId, spResult.ResultMessage);
        }
        else
        {
            LogPasswordChanged(logger, currentUser.LoginId);
        }

        return spResult.ToResult("Account.ChangePasswordFailed");
    }

    /// <inheritdoc />
    public async Task<Result> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await updateProfileValidator
            .ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var spResult = await accountRepository.UpdateProfileAsync(
            request,
            currentUser.LoginId,
            cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogProfileUpdateFailed(logger, currentUser.LoginId, spResult.ResultMessage);
        }
        else
        {
            LogProfileUpdated(logger, currentUser.LoginId);
        }

        return spResult.ToResult("Account.UpdateProfileFailed");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Profile retrieved for LoginId {LoginId}")]
    private static partial void LogProfileRetrieved(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Profile not found for LoginId {LoginId}")]
    private static partial void LogProfileNotFound(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password changed for LoginId {LoginId}")]
    private static partial void LogPasswordChanged(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Password change failed for LoginId {LoginId}: {Reason}")]
    private static partial void LogPasswordChangeFailed(ILogger logger, int loginId, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "Profile updated for LoginId {LoginId}")]
    private static partial void LogProfileUpdated(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Profile update failed for LoginId {LoginId}: {Reason}")]
    private static partial void LogProfileUpdateFailed(ILogger logger, int loginId, string reason);
}
