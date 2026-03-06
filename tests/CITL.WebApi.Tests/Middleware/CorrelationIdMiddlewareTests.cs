using CITL.SharedKernel.Constants;
using CITL.WebApi.Middleware;
using Microsoft.AspNetCore.Http;

namespace CITL.WebApi.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="CorrelationIdMiddleware"/>.
/// Verifies correlation ID generation, propagation, and response header behavior.
/// </summary>
public sealed class CorrelationIdMiddlewareTests
{
    private bool _nextCalled;
    private string? _capturedTraceIdentifier;

    private CorrelationIdMiddleware CreateMiddleware()
    {
        return new(
            context =>
            {
                _nextCalled = true;
                _capturedTraceIdentifier = context.TraceIdentifier;
                return Task.CompletedTask;
            });
    }

    private static DefaultHttpContext CreateContext(string? correlationId = null)
    {
        var context = new DefaultHttpContext();

        if (correlationId is not null)
        {
            context.Request.Headers[CorrelationConstants.HeaderName] = correlationId;
        }

        return context;
    }

    // ── Generation ────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_NoCorrelationHeader_GeneratesNewId()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(_nextCalled);
        Assert.NotNull(_capturedTraceIdentifier);
        Assert.True(Guid.TryParse(_capturedTraceIdentifier, out _));
    }

    [Fact]
    public async Task InvokeAsync_EmptyCorrelationHeader_GeneratesNewId()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext("   ");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(Guid.TryParse(_capturedTraceIdentifier, out _));
    }

    // ── Propagation ───────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WithCorrelationHeader_ReusesClientId()
    {
        // Arrange
        const string clientId = "client-trace-abc-123";
        var middleware = CreateMiddleware();
        var context = CreateContext(clientId);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(clientId, _capturedTraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_SetsTraceIdentifierOnHttpContext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext();
        var originalTraceId = context.TraceIdentifier;

        // Act
        await middleware.InvokeAsync(context);

        // Assert — new ID was set, different from original
        Assert.NotEqual(originalTraceId, context.TraceIdentifier);
    }

    // ── Response header ───────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdResponseHeader()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.TryGetValue(CorrelationConstants.HeaderName, out var headerValue));
        Assert.Equal(context.TraceIdentifier, headerValue.ToString());
    }

    [Fact]
    public async Task InvokeAsync_ResponseHeaderMatchesClientId()
    {
        // Arrange
        const string clientId = "propagated-id-xyz";
        var middleware = CreateMiddleware();
        var context = CreateContext(clientId);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(clientId, context.Response.Headers[CorrelationConstants.HeaderName].ToString());
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
}
