using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Application service for identity verification operations:
/// forgot password, password reset, email verification, and resend verification.
/// </summary>
public interface IIdentityVerificationService
{
    /// <summary>
    /// Initiates the forgot-password flow: verifies user identity (username + email + mobile),
    /// generates a reset token, and sends a password-reset email in the background.
    /// Always returns success to avoid leaking user existence information.
    /// </summary>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Resets a user's password using a valid reset token from the email link.
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Initiates the email verification flow: verifies user identity (username + email + mobile),
    /// generates a verification token, and sends a verification email in the background.
    /// Always returns success to avoid leaking user existence information.
    /// </summary>
    Task<Result> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies a user's email address using a valid verification token from the email link.
    /// </summary>
    Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);
}
