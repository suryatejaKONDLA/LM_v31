using System.Text.Json.Serialization;

namespace CITL.WebApi.Responses;

/// <summary>
/// Unified API response envelope for operations that do not return data.
/// Every endpoint returns this shape so the frontend can rely on a consistent contract.
/// </summary>
/// <remarks>
/// Status-code-specific subclasses add extra properties:
/// <list type="bullet">
///   <item><see cref="ApiResponse{T}"/> — adds <c>Data</c> (200 with payload)</item>
///   <item><see cref="ApiValidationResponse"/> — adds <c>Errors</c> (400 validation)</item>
///   <item><see cref="ApiErrorResponse"/> — adds <c>Exception</c> (500 server error)</item>
/// </list>
/// </remarks>
public class ApiResponse
{
    /// <summary>Unique correlation identifier for request tracing and support reference.</summary>
    [JsonPropertyName("RequestId")]
    [JsonPropertyOrder(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RequestId { get; set; }

    /// <summary>Machine-readable result code (1 = success, 0 = warning, -1 = error).</summary>
    [JsonPropertyName("Code")]
    [JsonPropertyOrder(0)]
    public int Code { get; init; }

    /// <summary>Result category ("success", "error", "warning", "info").</summary>
    [JsonPropertyName("Type")]
    [JsonPropertyOrder(1)]
    public string Type { get; init; } = ApiResponseType.Error;

    /// <summary>Human-readable message.</summary>
    [JsonPropertyName("Message")]
    [JsonPropertyOrder(2)]
    public string Message { get; init; } = string.Empty;

    /// <summary>UTC timestamp of the response.</summary>
    [JsonPropertyName("Timestamp")]
    [JsonPropertyOrder(3)]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    // ─── Factory methods ───────────────────────────────────────────────

    /// <summary>Creates a successful response.</summary>
    public static ApiResponse Success(string message = "Operation successful") =>
        new()
        {
            Code = ApiResponseCode.Success,
            Type = ApiResponseType.Success,
            Message = message,
        };

    /// <summary>Creates a warning response.</summary>
    public static ApiResponse Warn(string message) =>
        new()
        {
            Code = ApiResponseCode.Warning,
            Type = ApiResponseType.Warning,
            Message = message,
        };

    /// <summary>Creates an error response.</summary>
    public static ApiResponse Error(string message = "An error occurred") =>
        new()
        {
            Code = ApiResponseCode.Error,
            Type = ApiResponseType.Error,
            Message = message,
        };
}

/// <summary>
/// Unified API response envelope for operations that return data of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
public sealed class ApiResponse<T> : ApiResponse
{
    /// <summary>The response payload.</summary>
    [JsonPropertyName("Data")]
    [JsonPropertyOrder(6)]
    public T? Data { get; init; }

#pragma warning disable CA1000 // Do not declare static members on generic types — factory pattern is intentional

    /// <summary>Creates a successful response with data.</summary>
    public static ApiResponse<T> Success(T data, string message = "Operation successful") =>
        new()
        {
            Code = ApiResponseCode.Success,
            Type = ApiResponseType.Success,
            Message = message,
            Data = data,
        };

#pragma warning restore CA1000
}
