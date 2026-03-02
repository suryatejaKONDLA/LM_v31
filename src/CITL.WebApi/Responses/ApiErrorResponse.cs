using System.Text.Json.Serialization;

namespace CITL.WebApi.Responses;

/// <summary>
/// API response envelope for server errors (HTTP 500).
/// Contains exception detail in Development environments only.
/// </summary>
public sealed class ApiErrorResponse : ApiResponse
{
    /// <summary>Exception detail — included only in Development.</summary>
    [JsonPropertyName("Exception")]
    [JsonPropertyOrder(6)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExceptionDetail? Exception { get; init; }

    /// <summary>Creates a server error response with optional exception detail.</summary>
    public static ApiErrorResponse Create(
        string message = "An unexpected error occurred.",
        ExceptionDetail? exception = null) =>
        new()
        {
            Code = ApiResponseCode.Error,
            Type = ApiResponseType.Error,
            Message = message,
            Exception = exception,
        };
}
