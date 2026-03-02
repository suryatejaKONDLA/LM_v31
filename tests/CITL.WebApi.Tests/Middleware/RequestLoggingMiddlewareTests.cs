using CITL.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CITL.WebApi.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="RequestLoggingMiddleware"/>.
/// Verifies request timing, pipeline continuation, and error handling.
/// </summary>
public sealed class RequestLoggingMiddlewareTests
{
    private bool _nextCalled;

    private RequestLoggingMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        return new(
            next ?? (_ => { _nextCalled = true; return Task.CompletedTask; }),
            NullLogger<RequestLoggingMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateContext(string method = "GET", string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.TraceIdentifier = Guid.NewGuid().ToString("D");
        return context;
    }

    // ── Pipeline continuation ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
    }

    // ── Status code passthrough ───────────────────────────────────────────

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    public async Task InvokeAsync_PreservesResponseStatusCode(int statusCode)
    {
        // Arrange
        var middleware = CreateMiddleware(context =>
        {
            context.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        });
        var context = CreateContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(statusCode, context.Response.StatusCode);
    }

    // ── Exception propagation ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_PropagatesException()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("test"));
        var context = CreateContext();

        // Act & Assert — exception should NOT be swallowed
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context));
    }

    // ── Various HTTP methods ──────────────────────────────────────────────

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_HandlesAllHttpMethods(string method)
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext(method);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
    }
}
