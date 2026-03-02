namespace CITL.Application.Core.Authentication;

/// <summary>
/// Repository interface for identity verification operations:
/// forgot password, password reset, email verification, and user identity matching.
/// </summary>
public interface IIdentityVerificationRepository
{
    /// <summary>
    /// Verifies that the provided username, email, and mobile number match a single user record.
    /// Returns the user's identity info, or null if no match.
    /// </summary>
    Task<UserIdentityInfo?> VerifyUserIdentityAsync(
        string loginUser,
        string loginEmailId,
        string loginMobileNo,
        CancellationToken cancellationToken);

    /// <summary>
    /// Upserts a token into <c>citl.Login_Token_Store</c>.
    /// PK is (Login_ID, Token_Type), so a new token replaces any existing one.
    /// </summary>
    Task UpsertTokenAsync(int loginId, byte tokenType, string tokenValue, DateTime expiry, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a token and returns the associated Login_ID if found and not expired.
    /// Returns null if the token is invalid or expired.
    /// </summary>
    Task<int?> ValidateTokenAsync(byte tokenType, string tokenValue, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a token from the store after it has been consumed.
    /// </summary>
    Task DeleteTokenAsync(int loginId, byte tokenType, CancellationToken cancellationToken);

    /// <summary>
    /// Resets a user's password to the given plain-text value (SP hashes via SHA2_512)
    /// and sets <c>Login_Last_Password_Change_Date</c>.
    /// </summary>
    Task ResetPasswordAsync(int loginId, string newPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Sets <c>Login_Email_Verified_Flag = 1</c> for the given user.
    /// </summary>
    Task SetEmailVerifiedAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the user's display name by Login_ID (for email template rendering).
    /// </summary>
    Task<string?> GetLoginNameByIdAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes expired tokens across all types (housekeeping).
    /// </summary>
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken);
}
