using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Validation;
using CITL.Application.Core.Admin.AppMaster;
using CITL.Application.Core.Notifications.Email;
using CITL.Application.Core.Notifications.Email.Templates;
using CITL.SharedKernel.Helpers;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Orchestrates forgot-password, password-reset, email-verification, and resend-verification flows.
/// Sends emails in the background via <see cref="IBackgroundEmailDispatcher"/>.
/// </summary>
public sealed partial class IdentityVerificationService(
    IIdentityVerificationRepository repository,
    IAppMasterRepository appMasterRepository,
    IBackgroundEmailDispatcher emailDispatcher,
    ITenantContext tenantContext,
    IValidator<ForgotPasswordRequest> forgotValidator,
    IValidator<ResetPasswordRequest> resetValidator,
    IValidator<ResendVerificationRequest> resendValidator,
    IValidator<VerifyEmailRequest> verifyValidator,
    ILogger<IdentityVerificationService> logger) : IIdentityVerificationService
{
    /// <summary>Token type for email verification.</summary>
    private const byte EmailVerificationTokenType = 1;

    /// <summary>Token type for password reset.</summary>
    private const byte PasswordResetTokenType = 2;

    /// <summary>Password reset token expiry in hours.</summary>
    private const int PasswordResetExpiryHours = 1;

    /// <summary>Email verification token expiry in hours.</summary>
    private const int EmailVerificationExpiryHours = 24;

    /// <inheritdoc />
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await forgotValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var user = await repository.VerifyUserIdentityAsync(
            request.LoginUser, request.LoginEmailId, request.LoginMobileNo, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            // Do not reveal whether the user exists — always return success
            LogIdentityMismatch(logger, request.LoginUser);
            return Result.Success();
        }

        var token = CryptoHelper.GenerateBase64UrlToken(48);
        var expiry = DateTime.UtcNow.AddHours(PasswordResetExpiryHours);

        await repository.UpsertTokenAsync(user.LoginId, PasswordResetTokenType, token, expiry, cancellationToken)
            .ConfigureAwait(false);

        LogPasswordResetTokenCreated(logger, user.LoginId);

        var emailResult = await SendEmailInBackgroundAsync(
            user, token, PasswordResetTokenType, PasswordResetExpiryHours, cancellationToken)
            .ConfigureAwait(false);

        if (!emailResult.IsSuccess)
        {
            return emailResult;
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await resetValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var loginId = await repository.ValidateTokenAsync(PasswordResetTokenType, request.Token, cancellationToken)
            .ConfigureAwait(false);

        if (loginId is null)
        {
            return Result.Failure(new("Auth.InvalidToken", "The reset link is invalid or has expired. Please request a new one."));
        }

        await repository.ResetPasswordAsync(loginId.Value, request.LoginPassword, cancellationToken)
            .ConfigureAwait(false);

        await repository.DeleteTokenAsync(loginId.Value, PasswordResetTokenType, cancellationToken)
            .ConfigureAwait(false);

        LogPasswordResetCompleted(logger, loginId.Value);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var validation = await resendValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var user = await repository.VerifyUserIdentityAsync(
            request.LoginUser, request.LoginEmailId, request.LoginMobileNo, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            // Do not reveal whether the user exists
            LogIdentityMismatch(logger, request.LoginUser);
            return Result.Success();
        }

        if (user.LoginEmailVerifiedFlag)
        {
            return Result.Failure(new("Auth.AlreadyVerified", "This email address is already verified."));
        }

        var token = CryptoHelper.GenerateBase64UrlToken(48);
        var expiry = DateTime.UtcNow.AddHours(EmailVerificationExpiryHours);

        await repository.UpsertTokenAsync(user.LoginId, EmailVerificationTokenType, token, expiry, cancellationToken)
            .ConfigureAwait(false);

        LogVerificationTokenCreated(logger, user.LoginId);

        var emailResult = await SendEmailInBackgroundAsync(
            user, token, EmailVerificationTokenType, EmailVerificationExpiryHours, cancellationToken)
            .ConfigureAwait(false);

        return !emailResult.IsSuccess ? emailResult : Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var validation = await verifyValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var loginId = await repository.ValidateTokenAsync(EmailVerificationTokenType, request.Token, cancellationToken)
            .ConfigureAwait(false);

        if (loginId is null)
        {
            return Result.Failure(new("Auth.InvalidToken", "The verification link is invalid or has expired. Please request a new one."));
        }

        await repository.SetEmailVerifiedAsync(loginId.Value, cancellationToken)
            .ConfigureAwait(false);

        await repository.DeleteTokenAsync(loginId.Value, EmailVerificationTokenType, cancellationToken)
            .ConfigureAwait(false);

        LogEmailVerified(logger, loginId.Value);

        return Result.Success();
    }

    /// <summary>
    /// Resolves app branding, renders the email template, and dispatches the email in the background.
    /// </summary>
    private async Task<Result> SendEmailInBackgroundAsync(
        UserIdentityInfo user,
        string token,
        byte tokenType,
        int expiryHours,
        CancellationToken cancellationToken)
    {
        var appMaster = await appMasterRepository.GetAsync(cancellationToken).ConfigureAwait(false);
        var appName = appMaster?.AppHeader1 ?? "CITL";

        // Build inline logo for CID attachment (works in Gmail unlike data: URIs)
        List<InlineImage>? inlineImages = null;
        var hasLogo = false;

        if (appMaster?.AppLogo1 is not null)
        {
            hasLogo = true;
            inlineImages =
            [
                new InlineImage
                {
                    ContentId = "app-logo",
                    MimeType = ImageHelper.DetectMimeType(appMaster.AppLogo1),
                    Content = appMaster.AppLogo1
                }
            ];
        }

        var frontendBaseUrl = appMaster?.AppLink?.TrimEnd('/') ?? string.Empty;

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            LogAppLinkNotConfigured(logger, tenantContext.DatabaseName);
            return Result.Failure(new("AppMaster.AppLinkNotConfigured",
                "Application link is not configured for this tenant. Please update App Master settings."));
        }

        string template;
        string subject;
        string actionUrl;

        if (tokenType == PasswordResetTokenType)
        {
            template = EmailTemplates.PasswordReset;
            subject = $"Reset Your Password — {appName}";
            actionUrl = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        }
        else
        {
            template = EmailTemplates.EmailVerification;
            subject = $"Verify Your Email — {appName}";
            actionUrl = $"{frontendBaseUrl}/verify-email?token={Uri.EscapeDataString(token)}";
        }

        var htmlBody = EmailTemplates.Render(
            template, appName, hasLogo, user.LoginName, actionUrl, expiryHours);

        emailDispatcher.Enqueue(
            tenantContext.TenantId,
            tenantContext.DatabaseName,
            user.LoginEmailId,
            subject,
            htmlBody,
            inlineImages);

        return Result.Success();
    }

    // ── Source-generated log methods ─────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Identity verification failed — no match for user '{LoginUser}'")]
    private static partial void LogIdentityMismatch(ILogger logger, string loginUser);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Password reset token created for LoginId={LoginId}")]
    private static partial void LogPasswordResetTokenCreated(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Password reset completed for LoginId={LoginId}")]
    private static partial void LogPasswordResetCompleted(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email verification token created for LoginId={LoginId}")]
    private static partial void LogVerificationTokenCreated(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Email verified for LoginId={LoginId}")]
    private static partial void LogEmailVerified(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "APP_Link is not configured for tenant '{DatabaseName}' — email dispatch blocked")]
    private static partial void LogAppLinkNotConfigured(ILogger logger, string databaseName);
}
