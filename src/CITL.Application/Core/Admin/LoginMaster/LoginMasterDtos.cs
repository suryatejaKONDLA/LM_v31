using System.Text.Json.Serialization;
using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.LoginMaster;

/// <summary>
/// Request DTO for creating or updating a login.
/// </summary>
public sealed class LoginMasterRequest
{
    [JsonPropertyName("Login_ID")]
    public int LoginId { get; init; }

    [JsonPropertyName("Login_User")]
    public string LoginUser { get; init; } = string.Empty;

    [JsonPropertyName("Login_Name")]
    public string LoginName { get; init; } = string.Empty;

    [JsonPropertyName("Login_Designation")]
    public string LoginDesignation { get; init; } = string.Empty;

    [JsonPropertyName("Login_Mobile_No")]
    public string LoginMobileNo { get; init; } = string.Empty;

    [JsonPropertyName("Login_Email_ID")]
    public string LoginEmailId { get; init; } = string.Empty;

    [JsonPropertyName("Login_DOB")]
    public DateOnly? LoginDob { get; init; }

    [JsonPropertyName("Login_Gender")]
    public string LoginGender { get; init; } = string.Empty;

    [JsonPropertyName("Login_Active_Flag")]
    public bool LoginActiveFlag { get; init; } = true;

    [JsonPropertyName("BRANCH_Code")]
    public int BranchCode { get; init; }
}

/// <summary>
/// Response DTO for GET login endpoints.
/// </summary>
public sealed class LoginMasterResponse
{
    [JsonPropertyName("Login_ID")]
    public int LoginId { get; init; }

    [JsonPropertyName("Login_User")]
    public string LoginUser { get; init; } = string.Empty;

    [JsonPropertyName("Login_Name")]
    public string LoginName { get; init; } = string.Empty;

    [JsonPropertyName("Login_Branch_Code")]
    public int LoginBranchCode { get; init; }

    [JsonPropertyName("Login_Designation")]
    public string LoginDesignation { get; init; } = string.Empty;

    [JsonPropertyName("Login_Mobile_No")]
    public string LoginMobileNo { get; init; } = string.Empty;

    [JsonPropertyName("Login_Email_ID")]
    public string LoginEmailId { get; init; } = string.Empty;

    [JsonPropertyName("Login_DOB")]
    public DateOnly? LoginDob { get; init; }

    [JsonPropertyName("Login_Gender")]
    public string LoginGender { get; init; } = string.Empty;

    [JsonPropertyName("Login_Active_Flag")]
    public bool LoginActiveFlag { get; init; }

    [JsonPropertyName("Login_Created_Name")]
    public string? LoginCreatedName { get; init; }

    [JsonPropertyName("Login_Created_Date")]
    public DateTime? LoginCreatedDate { get; init; }

    [JsonPropertyName("Login_Modified_Name")]
    public string? LoginModifiedName { get; init; }

    [JsonPropertyName("Login_Modified_Date")]
    public DateTime? LoginModifiedDate { get; init; }

    [JsonPropertyName("Login_Approved_Name")]
    public string? LoginApprovedName { get; init; }

    [JsonPropertyName("Login_Approved_Date")]
    public DateTime? LoginApprovedDate { get; init; }
}

/// <summary>
/// Carries the stored procedure result plus the auto-generated password returned on insert.
/// </summary>
public sealed class LoginInsertSpResult
{
    /// <inheritdoc cref="SpResult.ResultVal"/>
    public int ResultVal { get; init; }

    /// <inheritdoc cref="SpResult.ResultType"/>
    public string ResultType { get; init; } = string.Empty;

    /// <inheritdoc cref="SpResult.ResultMessage"/>
    public string ResultMessage { get; init; } = string.Empty;

    /// <inheritdoc cref="SpResult.IsSuccess"/>
    public bool IsSuccess => string.Equals(ResultType, "SUCCESS", StringComparison.OrdinalIgnoreCase);

    /// <summary>Gets the auto-generated plain-text password (populated on new login insert only).</summary>
    public string ReturnPassword { get; init; } = string.Empty;
}
