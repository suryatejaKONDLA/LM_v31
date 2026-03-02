using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// Request DTO for creating or updating the company master configuration.
/// </summary>
/// <remarks>
/// <c>[JsonPropertyName]</c> ensures JSON property names match DB column names.
/// ASP.NET Core model binding is case-insensitive, so PascalCase C# names also work.
/// </remarks>
public sealed class CompanyMasterRequest
{
    /// <summary>Gets the company code.</summary>
    [JsonPropertyName("CMP_Code")]
    public required int CompanyCode { get; init; }

    /// <summary>Gets the full company name. Max 150 chars.</summary>
    [JsonPropertyName("CMP_Full_Name")]
    public required string FullName { get; init; }

    /// <summary>Gets the short company name. Max 30 chars.</summary>
    [JsonPropertyName("CMP_Short_Name")]
    public required string ShortName { get; init; }

    /// <summary>Gets the primary mobile number. Max 20 chars.</summary>
    [JsonPropertyName("CMP_Mobile1")]
    public string? Mobile1 { get; init; }

    /// <summary>Gets the secondary mobile number. Max 20 chars.</summary>
    [JsonPropertyName("CMP_Mobile2")]
    public string? Mobile2 { get; init; }

    /// <summary>Gets the company email address. Max 100 chars.</summary>
    [JsonPropertyName("CMP_Email")]
    public string? Email { get; init; }

    /// <summary>Gets the company website URL. Max 200 chars.</summary>
    [JsonPropertyName("CMP_Website")]
    public string? Website { get; init; }

    /// <summary>Gets the company tagline. Max 100 chars.</summary>
    [JsonPropertyName("CMP_Tagline")]
    public string? Tagline { get; init; }

    /// <summary>Gets logo image 1 as binary data.</summary>
    [JsonPropertyName("CMP_Logo1")]
    public byte[]? Logo1 { get; init; }

    /// <summary>Gets logo image 2 as binary data.</summary>
    [JsonPropertyName("CMP_Logo2")]
    public byte[]? Logo2 { get; init; }

    /// <summary>Gets logo image 3 as binary data.</summary>
    [JsonPropertyName("CMP_Logo3")]
    public byte[]? Logo3 { get; init; }

    /// <summary>Gets the session identifier.</summary>
    [JsonPropertyName("Session_Id")]
    public required int SessionId { get; init; }

    /// <summary>Gets the branch code.</summary>
    [JsonPropertyName("Branch_Code")]
    public required int BranchCode { get; init; }
}
