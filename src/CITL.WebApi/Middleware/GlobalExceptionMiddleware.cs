using System.Net;
using System.Net.Mime;
using System.Text.Json;
using CITL.SharedKernel.Exceptions;
using CITL.WebApi.Responses;

namespace CITL.WebApi.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns structured <see cref="ApiResponse"/> JSON.
/// Maps <see cref="AppException"/> subtypes to appropriate HTTP status codes.
/// Must be registered first in the middleware pipeline.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger.</param>
/// <param name="environment">The host environment (controls exception detail exposure).</param>
public sealed partial class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = MapException(exception);

        LogException(exception, statusCode);

        // Stamp correlation ID on the error response for client traceability
        response.RequestId = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, response.GetType(), JsonOptions));
    }

    private (int StatusCode, ApiResponse Response) MapException(Exception exception)
    {
        var exceptionDetail = environment.IsDevelopment()
            ? ExceptionDetail.FromException(exception)
            : null;

        return exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                ApiValidationResponse.Create(
                    [.. ex.Errors.Select(e => new FieldError { Field = e.Key, Messages = e.Value })],
                    ex.Message)),

            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ApiResponse.Error(ex.Message)),

            UnauthorizedException ex => (
                StatusCodes.Status401Unauthorized,
                ApiResponse.Error(ex.Message)),

            ForbiddenException ex => (
                StatusCodes.Status403Forbidden,
                ApiResponse.Error(ex.Message)),

            ConflictException ex => (
                StatusCodes.Status409Conflict,
                ApiResponse.Error(ex.Message)),

            TenantException ex => (
                StatusCodes.Status400BadRequest,
                ApiResponse.Error(ex.Message)),

            BadHttpRequestException ex => MapBadHttpRequestException(ex),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                ApiResponse.Error("The request was cancelled.")),

            _ => (
                StatusCodes.Status500InternalServerError,
                ApiErrorResponse.Create("An unexpected error occurred.", exceptionDetail))
        };
    }

    private static (int StatusCode, ApiResponse Response) MapBadHttpRequestException(
        BadHttpRequestException exception)
    {
        var message = exception.Message switch
        {
            _ when exception.Message.Contains("Request body too large", StringComparison.OrdinalIgnoreCase)
                => FormatFileSizeMessage(exception.Message),
            _ when exception.Message.Contains("multipart body length limit", StringComparison.OrdinalIgnoreCase)
                => "The uploaded file exceeds the maximum allowed size.",
            _ when exception.Message.Contains("form value length limit", StringComparison.OrdinalIgnoreCase)
                => "A form field value exceeds the maximum allowed length.",
            _ => "The request is invalid or malformed."
        };

        return (exception.StatusCode, ApiResponse.Error(message));
    }

    internal static string FormatFileSizeMessage(string originalMessage)
    {
        const long oneMb = 1_048_576;
        const long oneGb = 1_073_741_824;

        // Try to extract the byte limit from the original message
        var match = System.Text.RegularExpressions.Regex.Match(
            originalMessage, @"(\d+)\s*bytes", System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        if (match.Success && long.TryParse(match.Groups[1].Value, out var maxBytes))
        {
            var humanSize = maxBytes >= oneGb
                ? $"{maxBytes / oneGb} GB"
                : $"{maxBytes / oneMb} MB";

            return $"The request body exceeds the maximum allowed size of {humanSize}. Please upload a smaller file.";
        }

        return "The request body exceeds the maximum allowed size. Please upload a smaller file.";
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            LogUnhandledException(logger, exception.Message, exception);
        }
        else
        {
            LogHandledException(logger, statusCode, exception.Message);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception: {ErrorMessage}")]
    private static partial void LogUnhandledException(ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handled exception ({StatusCode}): {ErrorMessage}")]
    private static partial void LogHandledException(ILogger logger, int statusCode, string errorMessage);
}
