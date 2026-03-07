using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.FinYearMaster;

/// <summary>
/// Request DTO for creating/updating a financial year.
/// </summary>
public sealed class FinYearMasterRequest
{
    [JsonPropertyName("FIN_Year")]
    public int FinYear { get; init; }

    [JsonPropertyName("FIN_Date1")]
    public DateOnly FinDate1 { get; init; }

    [JsonPropertyName("FIN_Date2")]
    public DateOnly FinDate2 { get; init; }

    [JsonPropertyName("FIN_Active_Flag")]
    public bool FinActiveFlag { get; init; } = true;
}

/// <summary>
/// Response DTO for financial year GET endpoints.
/// </summary>
public sealed class FinYearResponse
{
    [JsonPropertyName("FIN_Year")]
    public int FinYear { get; init; }

    [JsonPropertyName("FIN_Date1")]
    public DateOnly FinDate1 { get; init; }

    [JsonPropertyName("FIN_Date2")]
    public DateOnly FinDate2 { get; init; }

    [JsonPropertyName("FIN_Active_Flag")]
    public bool FinActiveFlag { get; init; }
}
