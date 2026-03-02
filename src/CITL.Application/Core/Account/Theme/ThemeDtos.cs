using System.Text.Json.Serialization;

namespace CITL.Application.Core.Account.Theme;

/// <summary>
/// Response DTO for user theme configuration.
/// </summary>
public sealed class ThemeResponse
{
    /// <summary>Gets the login ID.</summary>
    [JsonPropertyName("Login_ID")]
    public int LoginId { get; init; }

    /// <summary>Gets the theme JSON containing token overrides.</summary>
    [JsonPropertyName("Theme_Json")]
    public string ThemeJson { get; init; } = string.Empty;
}

/// <summary>
/// Request DTO for saving user theme configuration.
/// </summary>
public sealed class SaveThemeRequest
{
    /// <summary>Gets the theme JSON containing token overrides.</summary>
    [JsonPropertyName("Theme_Json")]
    public required string ThemeJson { get; init; }
}
