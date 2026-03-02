using System.Text.Json.Serialization;

namespace CITL.WebApi.Responses;

/// <summary>
/// Represents a single field-level validation error.
/// </summary>
public sealed class FieldError
{
    /// <summary>Gets the field name that failed validation.</summary>
    [JsonPropertyName("Field")]
    public string Field { get; init; } = string.Empty;

    /// <summary>Gets the validation error messages for this field.</summary>
    [JsonPropertyName("Messages")]
    public string[] Messages { get; init; } = [];
}
