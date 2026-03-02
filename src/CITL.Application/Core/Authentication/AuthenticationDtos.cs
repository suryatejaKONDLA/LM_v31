using System.Text.Json.Serialization;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Request DTO for user login.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>Gets the username.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }

    /// <summary>Gets the password (plain text — hashed server-side by the SP via SHA2_512).</summary>
    [JsonPropertyName("Login_Password")]
    public required string LoginPassword { get; init; }

    /// <summary>Gets the client latitude from GPS/geolocation.</summary>
    [JsonPropertyName("Login_Latitude")]
    public decimal LoginLatitude { get; init; }

    /// <summary>Gets the client longitude from GPS/geolocation.</summary>
    [JsonPropertyName("Login_Longitude")]
    public decimal LoginLongitude { get; init; }

    /// <summary>Gets the geolocation accuracy in meters.</summary>
    [JsonPropertyName("Login_Accuracy")]
    public decimal LoginAccuracy { get; init; }

    /// <summary>Gets the client IP address.</summary>
    [JsonPropertyName("Login_IP")]
    public string LoginIp { get; init; } = string.Empty;

    /// <summary>Gets the client device identifier (browser user-agent or device name).</summary>
    [JsonPropertyName("Login_Device")]
    public string LoginDevice { get; init; } = string.Empty;

    /// <summary>Gets the CAPTCHA identifier (required when CAPTCHA is enforced after failed attempts).</summary>
    [JsonPropertyName("Captcha_Id")]
    public string? CaptchaId { get; init; }

    /// <summary>Gets the CAPTCHA answer (required when CAPTCHA is enforced after failed attempts).</summary>
    [JsonPropertyName("Captcha_Value")]
    public string? CaptchaValue { get; init; }
}

/// <summary>
/// Response DTO returned on successful login or token refresh.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>Gets the JWT access token.</summary>
    [JsonPropertyName("Access_Token")]
    public required string AccessToken { get; init; }

    /// <summary>Gets the refresh token for obtaining new access tokens.</summary>
    [JsonPropertyName("Refresh_Token")]
    public required string RefreshToken { get; init; }

    /// <summary>Gets the access token expiration time in UTC.</summary>
    [JsonPropertyName("Expires_At_Utc")]
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Gets the user's login ID.</summary>
    [JsonPropertyName("Login_Id")]
    public required int LoginId { get; init; }

    /// <summary>Gets the username.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }

    /// <summary>Gets the display name.</summary>
    [JsonPropertyName("Login_Name")]
    public required string LoginName { get; init; }

    /// <summary>Gets the user's roles.</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Gets all branches mapped to the user.</summary>
    public required IReadOnlyList<BranchInfo> Branches { get; init; }

    /// <summary>Gets a value indicating whether the user must change their password.</summary>
    [JsonPropertyName("Must_Change_Password")]
    public bool MustChangePassword { get; init; }
}

/// <summary>
/// Represents a branch mapped to a user via <c>citl.Login_Branch_Mapping</c>.
/// </summary>
public sealed class BranchInfo
{
    /// <summary>Gets the branch code.</summary>
    [JsonPropertyName("BRANCH_Code")]
    public int BranchCode { get; init; }

    /// <summary>Gets the branch name.</summary>
    [JsonPropertyName("BRANCH_Name")]
    public string BranchName { get; init; } = string.Empty;
}

/// <summary>
/// Request DTO for refreshing an access token.
/// </summary>
public sealed class RefreshTokenRequest
{
    /// <summary>Gets the current refresh token.</summary>
    [JsonPropertyName("Refresh_Token")]
    public required string RefreshToken { get; init; }

    /// <summary>Gets the username associated with the refresh token.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }
}

/// <summary>
/// Lightweight user profile for refresh-token and post-login lookup flows.
/// </summary>
public sealed class UserProfile
{
    /// <summary>Gets the login ID (primary key).</summary>
    public int LoginId { get; init; }

    /// <summary>Gets the login username.</summary>
    public string LoginUser { get; init; } = string.Empty;

    /// <summary>Gets the display name.</summary>
    public string LoginName { get; init; } = string.Empty;
}

/// <summary>
/// Represents a user role retrieved from <c>citl.Login_ROLE_Mapping</c>.
/// </summary>
public sealed class UserRole
{
    /// <summary>Gets the role name.</summary>
    public string RoleName { get; init; } = string.Empty;
}

/// <summary>
/// Request DTO for forgot password — user must provide username, email, and mobile for identity verification.
/// </summary>
public sealed class ForgotPasswordRequest
{
    /// <summary>Gets the login username.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }

    /// <summary>Gets the email address registered on the account.</summary>
    [JsonPropertyName("Login_Email_ID")]
    public required string LoginEmailId { get; init; }

    /// <summary>Gets the mobile number registered on the account.</summary>
    [JsonPropertyName("Login_Mobile_No")]
    public required string LoginMobileNo { get; init; }
}

/// <summary>
/// Request DTO for resetting a password using a token received via email.
/// </summary>
public sealed class ResetPasswordRequest
{
    /// <summary>Gets the reset token from the email link.</summary>
    public required string Token { get; init; }

    /// <summary>Gets the new password (plain text — hashed server-side by the SP via SHA2_512).</summary>
    [JsonPropertyName("Login_Password")]
    public required string LoginPassword { get; init; }
}

/// <summary>
/// Request DTO for email verification — user provides their email to receive a verification link.
/// </summary>
public sealed class ResendVerificationRequest
{
    /// <summary>Gets the login username.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }

    /// <summary>Gets the email address registered on the account.</summary>
    [JsonPropertyName("Login_Email_ID")]
    public required string LoginEmailId { get; init; }

    /// <summary>Gets the mobile number registered on the account.</summary>
    [JsonPropertyName("Login_Mobile_No")]
    public required string LoginMobileNo { get; init; }
}

/// <summary>
/// Request DTO for verifying an email address using a token from the verification link.
/// </summary>
public sealed class VerifyEmailRequest
{
    /// <summary>Gets the verification token from the email link.</summary>
    public required string Token { get; init; }
}

/// <summary>
/// Lightweight DTO for user identity verification (username + email + mobile match check).
/// </summary>
public sealed class UserIdentityInfo
{
    /// <summary>Gets the login ID.</summary>
    public int LoginId { get; init; }

    /// <summary>Gets the display name.</summary>
    public string LoginName { get; init; } = string.Empty;

    /// <summary>Gets the email address.</summary>
    public string LoginEmailId { get; init; } = string.Empty;

    /// <summary>Gets whether the email is already verified.</summary>
    public bool LoginEmailVerifiedFlag { get; init; }
}
