using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Application service orchestrating login, token refresh, and logout flows.
/// Uses FluentValidation, ITokenService, and IAuthenticationRepository.
/// </summary>
/// <param name="authRepository">The authentication repository.</param>
/// <param name="tokenService">The JWT token service.</param>
/// <param name="captchaService">The CAPTCHA service for generation and validation.</param>
/// <param name="loginValidator">The login request validator.</param>
/// <param name="logger">The logger.</param>
public sealed partial class AuthenticationService(
    IAuthenticationRepository authRepository,
    ITokenService tokenService,
    ICaptchaService captchaService,
    IValidator<LoginRequest> loginValidator,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    /// <inheritdoc />
    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        string tenantId,
        CancellationToken cancellationToken)
    {
        // 1. Validate input
        var validation = await loginValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult<LoginResponse>();
        }

        // 2. Check if CAPTCHA is required (based on failed attempt count)
        var captchaRequired = await captchaService.IsCaptchaRequiredAsync(
            request.LoginUser, cancellationToken).ConfigureAwait(false);

        if (captchaRequired)
        {
            if (string.IsNullOrWhiteSpace(request.CaptchaId) || string.IsNullOrWhiteSpace(request.CaptchaValue))
            {
                LogCaptchaRequired(logger, request.LoginUser, tenantId);
                return Result.Failure<LoginResponse>(
                    new("Captcha.Required", "CAPTCHA verification is required. Please complete the CAPTCHA."));
            }

            var captchaResult = await captchaService.ValidateAsync(
                request.CaptchaId, request.CaptchaValue, cancellationToken).ConfigureAwait(false);

            if (captchaResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(captchaResult.Error);
            }
        }

        // 3. Check credentials via SP (output params — no SELECT result set)
        var spResult = await authRepository.LoginCheckAsync(
            request,
            cancellationToken).ConfigureAwait(false);

        // SP returns ResultVal: 1 = success, 2 = password reset required, -1 = error
        if (spResult.ResultVal is not (1 or 2))
        {
            LogLoginFailed(logger, request.LoginUser, tenantId);
            return Result.Failure<LoginResponse>(
                Error.Validation("Authentication", spResult.ResultMessage));
        }

        // 4. Get user profile (Login_ID, Login_Name — SP doesn't return these)
        var profile = await authRepository.GetUserProfileAsync(request.LoginUser, cancellationToken).ConfigureAwait(false);

        if (profile is null)
        {
            return Result.Failure<LoginResponse>(
                Error.NotFound("User", request.LoginUser));
        }

        // 5. Get user roles and branches
        var roles = await authRepository.GetUserRolesAsync(profile.LoginId, cancellationToken).ConfigureAwait(false);
        var branches = await authRepository.GetUserBranchesAsync(profile.LoginId, cancellationToken).ConfigureAwait(false);

        // 6. Generate identity-only JWT (no roles/branch in token)
        var accessToken = tokenService.GenerateAccessToken(
            profile.LoginId,
            profile.LoginUser,
            profile.LoginName,
            tenantId);

        var refreshToken = tokenService.GenerateRefreshToken();

        // 7. Store refresh token (Redis + DB)
        await tokenService.StoreRefreshTokenAsync(
            profile.LoginUser,
            refreshToken,
            tenantId,
            cancellationToken).ConfigureAwait(false);

        LogLoginSucceeded(logger, profile.LoginUser, tenantId);

        return Result.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            LoginId = profile.LoginId,
            LoginUser = profile.LoginUser,
            LoginName = profile.LoginName,
            Roles = roles,
            Branches = branches,
            MustChangePassword = spResult.ResultVal is 2
        });
    }

    /// <inheritdoc />
    public async Task<Result<LoginResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string tenantId,
        CancellationToken cancellationToken)
    {
        // 1. Validate refresh token
        var validationResult = await tokenService.ValidateRefreshTokenAsync(
            request.LoginUser,
            request.RefreshToken,
            tenantId,
            cancellationToken).ConfigureAwait(false);

        if (validationResult.IsFailure)
        {
            LogRefreshFailed(logger, request.LoginUser, tenantId);
            return Result.Failure<LoginResponse>(validationResult.Error);
        }

        // 2. Get user profile (lightweight lookup — no password check)
        var profile = await authRepository.GetUserProfileAsync(request.LoginUser, cancellationToken).ConfigureAwait(false);

        if (profile is null)
        {
            return Result.Failure<LoginResponse>(
                Error.NotFound("User", request.LoginUser));
        }

        // 3. Get roles and branches (may have changed since last login)
        var roles = await authRepository.GetUserRolesAsync(profile.LoginId, cancellationToken).ConfigureAwait(false);
        var branches = await authRepository.GetUserBranchesAsync(profile.LoginId, cancellationToken).ConfigureAwait(false);

        // 4. Generate new identity-only token pair (rotation)
        var newAccessToken = tokenService.GenerateAccessToken(
            profile.LoginId,
            request.LoginUser,
            profile.LoginName,
            tenantId);

        var newRefreshToken = tokenService.GenerateRefreshToken();

        // 5. Store new refresh token (invalidates old one via MERGE in SP)
        await tokenService.StoreRefreshTokenAsync(
            request.LoginUser,
            newRefreshToken,
            tenantId,
            cancellationToken).ConfigureAwait(false);

        LogRefreshSucceeded(logger, request.LoginUser, tenantId);

        return Result.Success(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            LoginId = profile.LoginId,
            LoginUser = request.LoginUser,
            LoginName = profile.LoginName,
            Roles = roles,
            Branches = branches,
            MustChangePassword = false
        });
    }

    /// <inheritdoc />
    public async Task<Result> LogoutAsync(
        string accessToken,
        string loginUser,
        string tenantId,
        CancellationToken cancellationToken)
    {
        // 1. Blacklist the access token
        await tokenService.BlacklistTokenAsync(accessToken, tenantId, cancellationToken).ConfigureAwait(false);

        // 2. Invalidate the refresh token by storing a new random (effectively revoking the old)
        await tokenService.StoreRefreshTokenAsync(
            loginUser,
            tokenService.GenerateRefreshToken(),
            tenantId,
            cancellationToken).ConfigureAwait(false);

        LogLogout(logger, loginUser, tenantId);
        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Login succeeded — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogLoginSucceeded(ILogger logger, string loginUser, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login failed — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogLoginFailed(ILogger logger, string loginUser, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CAPTCHA required — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogCaptchaRequired(ILogger logger, string loginUser, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Token refresh succeeded — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogRefreshSucceeded(ILogger logger, string loginUser, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Token refresh failed — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogRefreshFailed(ILogger logger, string loginUser, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Logout — User: {LoginUser}, Tenant: {TenantId}")]
    private static partial void LogLogout(ILogger logger, string loginUser, string tenantId);
}
