using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Authentication;
using CITL.SharedKernel.Constants;
using CITL.WebApi.Constants;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers.Core.Authentication;

/// <summary>
/// Handles user authentication: login, token refresh, logout, CAPTCHA,
/// forgot password, password reset, and email verification.
/// </summary>
[Route("Auth")]
[ApiExplorerSettings(GroupName = ApiGroupConstants.Authentication)]
public sealed class AuthenticationController(
    IAuthenticationService authService,
    IIdentityVerificationService identityVerificationService,
    ICaptchaService captchaService,
    ITenantContext tenantContext) : CitlControllerBase
{
    /// <summary>
    /// Generates a CAPTCHA image if the user has exceeded the failed attempt threshold.
    /// Returns both dark and light theme images in a single call.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint — called before login when CAPTCHA may be required.
    /// If <c>CaptchaRequired</c> is <c>false</c>, the image fields are empty strings.
    /// </remarks>
    /// <param name="request">The request containing the username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>CAPTCHA images and metadata, or empty response if not required.</returns>
    /// <response code="200">CAPTCHA check completed — see <c>CaptchaRequired</c> flag.</response>
    [AllowAnonymous]
    [HttpPost("Captcha")]
    [ProducesResponseType(typeof(ApiResponse<CaptchaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCaptchaAsync(
        [FromBody] CaptchaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await captchaService.GenerateAsync(request, cancellationToken);
        return FromResult(result, result is { IsSuccess: true, Value.CaptchaRequired: true }
            ? "CAPTCHA generated successfully."
            : "CAPTCHA not required.");
    }

    /// <summary>
    /// Authenticates a user with credentials and returns JWT access + refresh tokens.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint — requires <c>X-Tenant-Id</c> header but no JWT.
    /// On success, returns access token (30 min), refresh token (30 days), user info,
    /// roles, and all branches mapped to the user.
    /// </remarks>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>JWT tokens, user info, roles, and branches on success.</returns>
    /// <response code="200">Login successful — returns tokens and user info.</response>
    /// <response code="400">Validation failed or invalid credentials.</response>
    [AllowAnonymous]
    [HttpPost("Login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, tenantContext.TenantId, cancellationToken);
        return FromResult(result, "Login successful.");
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// Implements refresh token rotation — old token is invalidated.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint — the refresh token itself acts as the credential.
    /// Returns updated roles and branches (may have changed since last login).
    /// </remarks>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>New JWT tokens on success.</returns>
    /// <response code="200">Token refresh successful — returns new tokens.</response>
    /// <response code="400">Invalid or expired refresh token.</response>
    [AllowAnonymous]
    [HttpPost("Refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, tenantContext.TenantId, cancellationToken);
        return FromResult(result, "Token refreshed successfully.");
    }

    /// <summary>
    /// Logs out the current user by blacklisting the access token and revoking the refresh token.
    /// </summary>
    /// <remarks>
    /// Requires a valid JWT. The access token is extracted from the Authorization header.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Logout successful.</response>
    [HttpPost("Logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var accessToken = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        var loginUser = User.FindFirst(AuthConstants.LoginUserClaimType)?.Value ?? string.Empty;

        var result = await authService.LogoutAsync(accessToken, loginUser, tenantContext.TenantId, cancellationToken);
        return FromResult(result, "Logout successful.");
    }

    /// <summary>
    /// Initiates the forgot-password flow. User must provide username, email, and mobile number.
    /// All three must match a record in the database before a reset email is sent.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint. Always returns 200 regardless of whether the user was found
    /// to prevent leaking user existence information.
    /// </remarks>
    /// <param name="request">The forgot password request with username, email, and mobile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Request processed — if valid, a reset email was sent.</response>
    /// <response code="400">Validation failed.</response>
    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await identityVerificationService.ForgotPasswordAsync(request, cancellationToken);
        return FromResult(result, "If your details are correct, a password reset link has been sent to your email.");
    }

    /// <summary>
    /// Resets a user's password using a valid reset token received via email.
    /// </summary>
    /// <param name="request">The reset request containing the token and new password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation or error if token is invalid/expired.</returns>
    /// <response code="200">Password reset successful.</response>
    /// <response code="400">Invalid/expired token or validation failed.</response>
    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await identityVerificationService.ResetPasswordAsync(request, cancellationToken);
        return FromResult(result, "Your password has been reset successfully. You can now log in with your new password.");
    }

    /// <summary>
    /// Resends a verification email. User must provide username, email, and mobile number.
    /// All three must match a record in the database before a verification email is sent.
    /// </summary>
    /// <remarks>
    /// Anonymous endpoint. Always returns 200 regardless of whether the user was found
    /// to prevent leaking user existence information.
    /// </remarks>
    /// <param name="request">The resend request with username, email, and mobile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Request processed — if valid, a verification email was sent.</response>
    /// <response code="400">Validation failed or email already verified.</response>
    [AllowAnonymous]
    [HttpPost("ResendVerification")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerificationAsync(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await identityVerificationService.ResendVerificationAsync(request, cancellationToken);
        return FromResult(result, "If your details are correct, a verification link has been sent to your email.");
    }

    /// <summary>
    /// Verifies a user's email address using a valid verification token from the email link.
    /// </summary>
    /// <param name="request">The verification request containing the token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success confirmation or error if token is invalid/expired.</returns>
    /// <response code="200">Email verified successfully.</response>
    /// <response code="400">Invalid/expired token or validation failed.</response>
    [AllowAnonymous]
    [HttpPost("VerifyEmail")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmailAsync(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await identityVerificationService.VerifyEmailAsync(request, cancellationToken);
        return FromResult(result, "Your email address has been verified successfully.");
    }
}
