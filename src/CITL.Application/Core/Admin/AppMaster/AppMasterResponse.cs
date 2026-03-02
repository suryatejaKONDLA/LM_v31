using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// Response DTO for the application master configuration.
/// Includes audit trail with resolved user names from <c>citl.Login_Name</c>.
/// </summary>
/// <remarks>
/// Dapper maps SQL underscore columns via <c>DefaultTypeMap.MatchNamesWithUnderscores</c>.
/// <c>[JsonPropertyName]</c> ensures JSON output matches DB column names.
/// </remarks>
public sealed class AppMasterResponse
{
    /// <summary>Gets the application code.</summary>
    [JsonPropertyName("APP_Code")]
    public int AppCode { get; init; }

    /// <summary>Gets the primary header (company/brand name).</summary>
    [JsonPropertyName("APP_Header1")]
    public required string AppHeader1 { get; init; }

    /// <summary>Gets the secondary header (short code).</summary>
    [JsonPropertyName("APP_Header2")]
    public required string AppHeader2 { get; init; }

    /// <summary>Gets the front-end application URL (e.g. https://www.citl.co.in/pos/).</summary>
    [JsonPropertyName("APP_Link")]
    public required string AppLink { get; init; }

    /// <summary>Gets logo image 1.</summary>
    [JsonPropertyName("APP_Logo1")]
    public byte[]? AppLogo1 { get; init; }

    /// <summary>Gets logo image 2.</summary>
    [JsonPropertyName("APP_Logo2")]
    public byte[]? AppLogo2 { get; init; }

    /// <summary>Gets logo image 3.</summary>
    [JsonPropertyName("APP_Logo3")]
    public byte[]? AppLogo3 { get; init; }

    /// <summary>Gets the creator user ID.</summary>
    [JsonPropertyName("APP_Created_ID")]
    public int AppCreatedId { get; init; }

    /// <summary>Gets the creator user name (resolved from Login_Name).</summary>
    [JsonPropertyName("APP_Created_Name")]
    public string? AppCreatedName { get; init; }

    /// <summary>Gets the creation date.</summary>
    [JsonPropertyName("APP_Created_Date")]
    public DateTime AppCreatedDate { get; init; }

    /// <summary>Gets the modifier user ID.</summary>
    [JsonPropertyName("APP_Modified_ID")]
    public int? AppModifiedId { get; init; }

    /// <summary>Gets the modifier user name (resolved from Login_Name).</summary>
    [JsonPropertyName("APP_Modified_Name")]
    public string? AppModifiedName { get; init; }

    /// <summary>Gets the modification date.</summary>
    [JsonPropertyName("APP_Modified_Date")]
    public DateTime? AppModifiedDate { get; init; }

    /// <summary>Gets the approver user ID.</summary>
    [JsonPropertyName("APP_Approved_ID")]
    public int? AppApprovedId { get; init; }

    /// <summary>Gets the approver user name (resolved from Login_Name).</summary>
    [JsonPropertyName("APP_Approved_Name")]
    public string? AppApprovedName { get; init; }

    /// <summary>Gets the approval date.</summary>
    [JsonPropertyName("APP_Approved_Date")]
    public DateTime? AppApprovedDate { get; init; }
}
