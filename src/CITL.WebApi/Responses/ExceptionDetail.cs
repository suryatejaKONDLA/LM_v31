using System.Text.Json.Serialization;

namespace CITL.WebApi.Responses;

/// <summary>
/// Serializable exception detail included only in Development environments.
/// </summary>
public sealed class ExceptionDetail
{
    /// <summary>Gets the exception type name.</summary>
    [JsonPropertyName("Type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the exception message.</summary>
    [JsonPropertyName("Message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the source of the exception.</summary>
    [JsonPropertyName("Source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; init; }

    /// <summary>Gets the stack trace.</summary>
    [JsonPropertyName("StackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; init; }

    /// <summary>Gets the target site name.</summary>
    [JsonPropertyName("TargetSite")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetSite { get; init; }

    /// <summary>Gets the inner exception detail.</summary>
    [JsonPropertyName("InnerException")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExceptionDetail? InnerException { get; init; }

    /// <summary>
    /// Creates an <see cref="ExceptionDetail"/> from the given exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>A serializable representation, or <see langword="null"/> if the input is null.</returns>
    public static ExceptionDetail? FromException(Exception? exception)
    {
        if (exception is null)
        {
            return null;
        }

        return new()
        {
            Type = exception.GetType().FullName ?? exception.GetType().Name,
            Message = exception.Message,
            Source = exception.Source,
            StackTrace = exception.StackTrace,
            TargetSite = exception.TargetSite?.ToString(),
            InnerException = FromException(exception.InnerException),
        };
    }
}
