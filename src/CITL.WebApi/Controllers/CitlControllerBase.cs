using System.Net.Mime;
using CITL.SharedKernel.Results;
using CITL.WebApi.Extensions;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Controllers;

/// <summary>
/// Base controller providing unified <see cref="ApiResponse"/> helpers.
/// All CITL controllers should inherit from this instead of <see cref="ControllerBase"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Authorization</b>: All endpoints require a valid JWT by default.
/// Mark individual actions or controllers with <see cref="AllowAnonymousAttribute"/>
/// to opt out (e.g., Login, Refresh).
/// </para>
/// <para>
/// Common response types (401, 403, 409, 500) are declared here so every
/// endpoint documents them automatically. Action-specific codes (200, 201, 400, 404)
/// should be declared on individual action methods.
/// </para>
/// </remarks>
[Authorize]
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public abstract class CitlControllerBase : ControllerBase
{
    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to an <see cref="IActionResult"/>
    /// wrapped in <see cref="ApiResponse"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    protected IActionResult FromResult(Result result, string? successMessage = null) =>
        result.ToActionResult(successMessage);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IActionResult"/>
    /// wrapped in <see cref="ApiResponse{T}"/>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    protected IActionResult FromResult<T>(Result<T> result, string? successMessage = null) =>
        result.ToActionResult(successMessage);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a 201 Created <see cref="IActionResult"/>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="location">The URI of the created resource.</param>
    /// <param name="successMessage">Optional success message.</param>
    /// <returns>An <see cref="IActionResult"/>.</returns>
    protected IActionResult FromCreatedResult<T>(
        Result<T> result,
        string? location = null,
        string? successMessage = null) =>
        result.ToCreatedResult(location, successMessage);
}
