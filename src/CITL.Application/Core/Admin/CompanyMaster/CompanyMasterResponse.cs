using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// Response DTO for the company master configuration.
/// Includes audit trail with resolved user names from <c>citl.Login_Name</c>.
/// </summary>
/// <remarks>
/// Dapper maps SQL underscore columns via <c>DefaultTypeMap.MatchNamesWithUnderscores</c>.
/// <c>[JsonPropertyName]</c> ensures JSON output matches DB column names.
/// </remarks>
public sealed class CompanyMasterResponse
{
    /// <summary>Gets the company code.</summary>
    [JsonPropertyName("CMP_Code")]
    public int CmpCode { get; init; }

    /// <summary>Gets the full company name.</summary>
    [JsonPropertyName("CMP_Full_Name")]
    public required string CmpFullName { get; init; }

    /// <summary>Gets the short company name.</summary>
    [JsonPropertyName("CMP_Short_Name")]
    public required string CmpShortName { get; init; }

    /// <summary>Gets the primary mobile number.</summary>
    [JsonPropertyName("CMP_Mobile1")]
    public string? CmpMobile1 { get; init; }

    /// <summary>Gets the secondary mobile number.</summary>
    [JsonPropertyName("CMP_Mobile2")]
    public string? CmpMobile2 { get; init; }

    /// <summary>Gets the company email address.</summary>
    [JsonPropertyName("CMP_Email")]
    public string? CmpEmail { get; init; }

    /// <summary>Gets the company website URL.</summary>
    [JsonPropertyName("CMP_Website")]
    public string? CmpWebsite { get; init; }

    /// <summary>Gets the company tagline.</summary>
    [JsonPropertyName("CMP_Tagline")]
    public string? CmpTagline { get; init; }

    /// <summary>Gets logo image 1.</summary>
    [JsonPropertyName("CMP_Logo1")]
    public byte[]? CmpLogo1 { get; init; }

    /// <summary>Gets logo image 2.</summary>
    [JsonPropertyName("CMP_Logo2")]
    public byte[]? CmpLogo2 { get; init; }

    /// <summary>Gets logo image 3.</summary>
    [JsonPropertyName("CMP_Logo3")]
    public byte[]? CmpLogo3 { get; init; }

    /// <summary>Gets the creator user ID.</summary>
    [JsonPropertyName("CMP_Created_ID")]
    public int CmpCreatedId { get; init; }

    /// <summary>Gets the creator user name (resolved from Login_Name).</summary>
    [JsonPropertyName("CMP_Created_Name")]
    public string? CmpCreatedName { get; init; }

    /// <summary>Gets the creation date.</summary>
    [JsonPropertyName("CMP_Created_Date")]
    public DateTime CmpCreatedDate { get; init; }

    /// <summary>Gets the modifier user ID.</summary>
    [JsonPropertyName("CMP_Modified_ID")]
    public int? CmpModifiedId { get; init; }

    /// <summary>Gets the modifier user name (resolved from Login_Name).</summary>
    [JsonPropertyName("CMP_Modified_Name")]
    public string? CmpModifiedName { get; init; }

    /// <summary>Gets the modification date.</summary>
    [JsonPropertyName("CMP_Modified_Date")]
    public DateTime? CmpModifiedDate { get; init; }

    /// <summary>Gets the approver user ID.</summary>
    [JsonPropertyName("CMP_Approved_ID")]
    public int? CmpApprovedId { get; init; }

    /// <summary>Gets the approver user name (resolved from Login_Name).</summary>
    [JsonPropertyName("CMP_Approved_Name")]
    public string? CmpApprovedName { get; init; }

    /// <summary>Gets the approval date.</summary>
    [JsonPropertyName("CMP_Approved_Date")]
    public DateTime? CmpApprovedDate { get; init; }
}
