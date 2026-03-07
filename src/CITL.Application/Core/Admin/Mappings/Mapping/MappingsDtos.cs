using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.Mappings.Mapping;

/// <summary>
/// Request DTO for inserting or updating a generic anchor-to-item mapping.
/// </summary>
public sealed class MappingsRequest
{
    [JsonPropertyName("queryString")]
    public string QueryString { get; init; } = string.Empty;

    [JsonPropertyName("swapFlag")]
    public int SwapFlag { get; init; }

    [JsonPropertyName("anchorId")]
    public string AnchorId { get; init; } = string.Empty;

    [JsonPropertyName("mappingIds")]
    public IList<string> MappingIds { get; init; } = [];
}

/// <summary>
/// Response DTO for a single mapping row (generic left/right columns).
/// </summary>
public sealed class MappingsResponse
{
    [JsonPropertyName("Left_Column")]
    public string LeftColumn { get; init; } = string.Empty;

    [JsonPropertyName("Right_Column")]
    public string RightColumn { get; init; } = string.Empty;
}
