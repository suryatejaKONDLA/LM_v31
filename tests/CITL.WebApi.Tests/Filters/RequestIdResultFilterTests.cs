using CITL.WebApi.Filters;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CITL.WebApi.Tests.Filters;

/// <summary>
/// Unit tests for <see cref="RequestIdResultFilter"/>.
/// Verifies that RequestId is stamped on ApiResponse objects.
/// </summary>
public sealed class RequestIdResultFilterTests
{
    private const string TestCorrelationId = "test-correlation-id-12345";

    private static ResultExecutingContext CreateContext(IActionResult result)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = TestCorrelationId;

        var actionContext = new ActionContext(
            httpContext,
            new(),
            new());

        return new(
            actionContext,
            [],
            result,
            controller: null!);
    }

    // ── ApiResponse stamping ──────────────────────────────────────────────

    [Fact]
    public void OnResultExecuting_ApiResponse_SetsRequestId()
    {
        // Arrange
        var response = ApiResponse.Success();
        var context = CreateContext(new OkObjectResult(response));
        var filter = new RequestIdResultFilter();

        // Act
        filter.OnResultExecuting(context);

        // Assert
        Assert.Equal(TestCorrelationId, response.RequestId);
    }

    [Fact]
    public void OnResultExecuting_ApiResponseOfT_SetsRequestId()
    {
        // Arrange
        var response = ApiResponse<string>.Success("test-data");
        var context = CreateContext(new OkObjectResult(response));
        var filter = new RequestIdResultFilter();

        // Act
        filter.OnResultExecuting(context);

        // Assert
        Assert.Equal(TestCorrelationId, response.RequestId);
    }

    [Fact]
    public void OnResultExecuting_ApiErrorResponse_SetsRequestId()
    {
        // Arrange
        var response = ApiErrorResponse.Create("Something went wrong");
        var context = CreateContext(new ObjectResult(response) { StatusCode = 500 });
        var filter = new RequestIdResultFilter();

        // Act
        filter.OnResultExecuting(context);

        // Assert
        Assert.Equal(TestCorrelationId, response.RequestId);
    }

    [Fact]
    public void OnResultExecuting_ApiValidationResponse_SetsRequestId()
    {
        // Arrange
        var response = ApiValidationResponse.Create(
            [new() { Field = "Name", Messages = ["Required"] }]);
        var context = CreateContext(new BadRequestObjectResult(response));
        var filter = new RequestIdResultFilter();

        // Act
        filter.OnResultExecuting(context);

        // Assert
        Assert.Equal(TestCorrelationId, response.RequestId);
    }

    // ── Non-ApiResponse passthrough ───────────────────────────────────────

    [Fact]
    public void OnResultExecuting_NonApiResponse_DoesNotThrow()
    {
        // Arrange
        var context = CreateContext(new OkObjectResult("plain string"));
        var filter = new RequestIdResultFilter();

        // Act & Assert — should not throw
        filter.OnResultExecuting(context);
    }

    [Fact]
    public void OnResultExecuting_NonObjectResult_DoesNotThrow()
    {
        // Arrange
        var context = CreateContext(new StatusCodeResult(204));
        var filter = new RequestIdResultFilter();

        // Act & Assert — should not throw
        filter.OnResultExecuting(context);
    }

    // ── OnResultExecuted no-op ────────────────────────────────────────────

    [Fact]
    public void OnResultExecuted_DoesNotThrow()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new(), new());
        var context = new ResultExecutedContext(
            actionContext, [], new OkResult(), controller: null!);
        var filter = new RequestIdResultFilter();

        // Act & Assert
        filter.OnResultExecuted(context);
    }
}
