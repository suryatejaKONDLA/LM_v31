using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// Request DTO for creating or updating the application master configuration.
/// </summary>
/// <remarks>
/// <c>[JsonPropertyName]</c> ensures JSON property names match DB column names.
/// ASP.NET Core model binding is case-insensitive, so PascalCase C# names also work.
/// </remarks>
public sealed class AppMasterRequest
{
    /// <summary>Gets the application code.</summary>
    [JsonPropertyName("APP_Code")]
    public required int AppCode { get; init; }

    /// <summary>Gets the primary header (company/brand name). Max 60 chars.</summary>
    [JsonPropertyName("APP_Header1")]
    public required string AppHeader1 { get; init; }

    /// <summary>Gets the secondary header (short code). Max 7 chars, alphanumeric only.</summary>
    [JsonPropertyName("APP_Header2")]
    public required string AppHeader2 { get; init; }

    /// <summary>Gets the front-end application URL. Max 500 chars.</summary>
    [JsonPropertyName("APP_Link")]
    public string? AppLink { get; init; }

    /// <summary>Gets logo image 1 as binary data.</summary>
    [JsonPropertyName("APP_Logo1")]
    public byte[]? AppLogo1 { get; init; }

    /// <summary>Gets logo image 2 as binary data.</summary>
    [JsonPropertyName("APP_Logo2")]
    public byte[]? AppLogo2 { get; init; }

    /// <summary>Gets logo image 3 as binary data.</summary>
    [JsonPropertyName("APP_Logo3")]
    public byte[]? AppLogo3 { get; init; }

    /// <summary>Gets the session identifier.</summary>
    [JsonPropertyName("Session_Id")]
    public required int SessionId { get; init; }

    /// <summary>Gets the branch code.</summary>
    [JsonPropertyName("Branch_Code")]
    public required int BranchCode { get; init; }
}
