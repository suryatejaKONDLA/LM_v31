using CITL.SharedKernel.Results;
using CITL.WebApi.Extensions;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CITL.WebApi.Tests.Extensions;

/// <summary>
/// Unit tests for <see cref="ResultExtensions"/>.
/// Verifies correct HTTP status code mapping for all Result states.
/// </summary>
public sealed class ResultExtensionsTests
{
    // ── ToActionResult (non-generic) ──────────────────────────────────────────

    [Fact]
    public void ToActionResult_WhenSuccess_WithoutMessage_Returns200WithApiResponse()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, response.Code);
    }

    [Fact]
    public void ToActionResult_WhenSuccess_WithMessage_Returns200OkWithMessage()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = result.ToActionResult("Saved successfully.");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal("Saved successfully.", response.Message);
    }

    [Fact]
    public void ToActionResult_WhenNotFoundError_Returns404()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound("User", 42));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_WhenConflictError_Returns409()
    {
        // Arrange
        var result = Result.Failure(Error.Conflict("User", "Duplicate email."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(409, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_WhenValidationError_Returns400()
    {
        // Arrange
        var result = Result.Failure(Error.Validation("Name", "Required."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_WhenGenericError_Returns400()
    {
        // Arrange
        var result = Result.Failure(new("SomeError", "Something went wrong."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── ToActionResult<T> ─────────────────────────────────────────────────────

    [Fact]
    public void ToActionResultT_WhenSuccess_Returns200WithApiResponseT()
    {
        // Arrange
        var result = Result.Success(new { Id = 1, Name = "Test" });

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void ToActionResultT_WhenNotFoundError_Returns404()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound("Item", "abc"));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResultT_WhenConflictError_Returns409()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Conflict("Order", "Already exists."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(409, objectResult.StatusCode);
    }

    // ── ToCreatedResult<T> ────────────────────────────────────────────────────

    [Fact]
    public void ToCreatedResult_WhenSuccess_WithLocation_Returns201WithLocation()
    {
        // Arrange
        var result = Result.Success(new { Id = 99 });

        // Act
        var actionResult = result.ToCreatedResult("/api/items/99");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("/api/items/99", createdResult.Location);
        Assert.NotNull(createdResult.Value);
    }

    [Fact]
    public void ToCreatedResult_WhenSuccess_WithoutLocation_Returns201()
    {
        // Arrange
        var result = Result.Success("created");

        // Act
        var actionResult = result.ToCreatedResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact]
    public void ToCreatedResult_WhenFailure_ReturnsErrorResult()
    {
        // Arrange
        var result = Result.Failure<string>(new("Create.Failed", "Oops."));

        // Act
        var actionResult = result.ToCreatedResult("/api/items/1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }
}
