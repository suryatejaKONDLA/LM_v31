using CITL.SharedKernel.Results;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Extensions;

/// <summary>
/// Extension methods for converting <see cref="Result"/> and <see cref="Result{T}"/>
/// to <see cref="IActionResult"/> wrapped in <see cref="ApiResponse"/>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to an <see cref="IActionResult"/>.
    /// Returns 200 OK with <see cref="ApiResponse"/> on success, or the appropriate error.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    public static IActionResult ToActionResult(this Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(
                ApiResponse.Success(successMessage ?? "Operation successful"));
        }

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IActionResult"/>.
    /// Returns 200 OK with <see cref="ApiResponse{T}"/> on success, or the appropriate error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    public static IActionResult ToActionResult<T>(this Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(
                ApiResponse<T>.Success(result.Value!, successMessage ?? "Operation successful"));
        }

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IActionResult"/>
    /// with a 201 Created status on success.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="location">The URI of the created resource.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string? location = null,
        string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Success(
                result.Value!, successMessage ?? "Resource created successfully");

            return location is not null
                ? new CreatedResult(location, response)
                : new ObjectResult(response) { StatusCode = StatusCodes.Status201Created };
        }

        return ToErrorResult(result.Error);
    }

    private static ObjectResult ToErrorResult(Error error)
    {
        var statusCode = error.Code switch
        {
            var c when c.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                => StatusCodes.Status404NotFound,
            var c when c.Contains("Conflict", StringComparison.OrdinalIgnoreCase)
                => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return new(ApiResponse.Error(error.Description))
        {
            StatusCode = statusCode
        };
    }
}
