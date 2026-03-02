using System.Text.Json.Serialization;

namespace CITL.WebApi.Responses;

/// <summary>
/// API response envelope for validation failures (HTTP 400).
/// Contains field-level error details.
/// </summary>
public sealed class ApiValidationResponse : ApiResponse
{
    /// <summary>Field-level validation errors.</summary>
    [JsonPropertyName("Errors")]
    [JsonPropertyOrder(6)]
    public IReadOnlyList<FieldError> Errors { get; init; } = [];

    /// <summary>Creates a validation error response with field-level errors.</summary>
    public static ApiValidationResponse Create(
        IReadOnlyList<FieldError> errors,
        string message = "Validation failed") =>
        new()
        {
            Code = ApiResponseCode.Error,
            Type = ApiResponseType.Error,
            Message = message,
            Errors = errors,
        };
}
