using System.Text.Json.Serialization;

namespace CITL.Application.Common.Models;

/// <summary>
/// Reusable dropdown response DTO for populating UI select lists.
/// </summary>
/// <typeparam name="T">The type of the identifier (typically <see langword="int"/>).</typeparam>
public sealed class DropDownResponse<T>
{
    /// <summary>The dropdown value (identifier).</summary>
    [JsonPropertyName("Col1")]
    public T Value { get; init; } = default!;

    /// <summary>The dropdown display text.</summary>
    [JsonPropertyName("Col2")]
    public string Text { get; init; } = string.Empty;
}
