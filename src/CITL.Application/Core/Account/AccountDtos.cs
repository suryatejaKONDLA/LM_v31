using System.Text.Json.Serialization;

namespace CITL.Application.Core.Account;

/// <summary>
/// Request DTO for changing the current user's password.
/// </summary>
public sealed class ChangePasswordRequest
{
    /// <summary>Gets the current password (plain text — hashed server-side by the SP via SHA2_512).</summary>
    [JsonPropertyName("Login_Password_Old")]
    public required string LoginPasswordOld { get; init; }

    /// <summary>Gets the new password (plain text — hashed server-side by the SP via SHA2_512).</summary>
    [JsonPropertyName("Login_Password")]
    public required string LoginPassword { get; init; }
}

/// <summary>
/// Request DTO for updating the current user's profile.
/// </summary>
public sealed class UpdateProfileRequest
{
    /// <summary>Gets the display name.</summary>
    [JsonPropertyName("Login_Name")]
    public required string LoginName { get; init; }

    /// <summary>Gets the mobile number.</summary>
    [JsonPropertyName("Login_Mobile_No")]
    public string LoginMobileNo { get; init; } = string.Empty;

    /// <summary>Gets the email address.</summary>
    [JsonPropertyName("Login_Email_ID")]
    public string LoginEmailId { get; init; } = string.Empty;

    /// <summary>Gets the date of birth.</summary>
    [JsonPropertyName("Login_DOB")]
    public DateTime? LoginDob { get; init; }

    /// <summary>Gets the profile picture as a base64-encoded string.</summary>
    [JsonPropertyName("Login_Pic")]
    public string? LoginPic { get; init; }

    /// <summary>Gets the startup page menu ID.</summary>
    [JsonPropertyName("Menu_ID")]
    public string MenuId { get; init; } = "0";
}

/// <summary>
/// Response DTO for get profile.
/// </summary>
public sealed class ProfileResponse
{
    /// <summary>Gets the login ID.</summary>
    [JsonPropertyName("Login_Id")]
    public int LoginId { get; init; }

    /// <summary>Gets the login username.</summary>
    [JsonPropertyName("Login_User")]
    public string LoginUser { get; init; } = string.Empty;

    /// <summary>Gets the display name.</summary>
    [JsonPropertyName("Login_Name")]
    public string LoginName { get; init; } = string.Empty;

    /// <summary>Gets the default branch code.</summary>
    [JsonPropertyName("Login_Branch_Code")]
    public int LoginBranchCode { get; init; }

    /// <summary>Gets the designation.</summary>
    [JsonPropertyName("Login_Designation")]
    public string LoginDesignation { get; init; } = string.Empty;

    /// <summary>Gets the mobile number.</summary>
    [JsonPropertyName("Login_Mobile_No")]
    public string LoginMobileNo { get; init; } = string.Empty;

    /// <summary>Gets the email address.</summary>
    [JsonPropertyName("Login_Email_ID")]
    public string LoginEmailId { get; init; } = string.Empty;

    /// <summary>Gets the date of birth.</summary>
    [JsonPropertyName("Login_DOB")]
    public DateTime? LoginDob { get; init; }

    /// <summary>Gets the gender (M/F/O).</summary>
    [JsonPropertyName("Login_Gender")]
    public string LoginGender { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the email is verified.</summary>
    [JsonPropertyName("Login_Email_Verified")]
    public bool LoginEmailVerified { get; init; }

    /// <summary>Gets the profile picture as a base64-encoded string.</summary>
    [JsonPropertyName("Login_Pic")]
    public string? LoginPic { get; init; }

    /// <summary>Gets the startup page menu ID.</summary>
    [JsonPropertyName("Menu_ID")]
    public string? MenuId { get; init; }
}
