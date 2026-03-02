using System.Text.Json;
using CITL.SharedKernel.Exceptions;
using CITL.WebApi.Middleware;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.WebApi.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="GlobalExceptionMiddleware"/>.
/// Verifies all exception-to-HTTP-status-code mappings.
/// </summary>
public sealed class GlobalExceptionMiddlewareTests
{
    private static readonly IHostEnvironment DevEnvironment = CreateEnvironment("Development");

    private static IHostEnvironment CreateEnvironment(string environmentName)
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);
        return env;
    }

    private static (GlobalExceptionMiddleware Middleware, DefaultHttpContext Context) Create(
        Exception exception, IHostEnvironment? environment = null)
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionMiddleware>.Instance,
            environment ?? DevEnvironment);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        return (middleware, context);
    }

    private static (GlobalExceptionMiddleware Middleware, DefaultHttpContext Context) CreateSuccess()
    {
        var middleware = new GlobalExceptionMiddleware(
            ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            },
            NullLogger<GlobalExceptionMiddleware>.Instance,
            DevEnvironment);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        return (middleware, context);
    }

    private static async Task<(int StatusCode, JsonDocument Body)> InvokeAndReadAsync(
        GlobalExceptionMiddleware middleware, DefaultHttpContext context)
    {
        await middleware.InvokeAsync(context).ConfigureAwait(false);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

        var body = json.Length > 0
            ? JsonDocument.Parse(json)
            : JsonDocument.Parse("{}");

        return (context.Response.StatusCode, body);
    }

    // ── Success (no exception) ────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenNoException_PassesThrough()
    {
        // Arrange
        var (middleware, context) = CreateSuccess();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
    }

    // ── ValidationException → 400 ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        // Arrange
        var (middleware, context) = Create(new ValidationException("Name", "Required."));

        // Act
        var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(400, statusCode);
        Assert.Equal(ApiResponseCode.Error, body.RootElement.GetProperty("Code").GetInt32());
        Assert.Equal(ApiResponseType.Error, body.RootElement.GetProperty("Type").GetString());
        Assert.True(body.RootElement.TryGetProperty("Errors", out _));
        Assert.False(body.RootElement.TryGetProperty("Exception", out _));
    }

    // ── NotFoundException → 404 ───────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        var (middleware, context) = Create(new NotFoundException("User", 42));

        // Act
        var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(404, statusCode);
        Assert.Equal(ApiResponseCode.Error, body.RootElement.GetProperty("Code").GetInt32());
    }

    // ── UnauthorizedException → 401 ───────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        var (middleware, context) = Create(new UnauthorizedException());

        // Act
        var (statusCode, _) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(401, statusCode);
    }

    // ── ForbiddenException → 403 ──────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403()
    {
        // Arrange
        var (middleware, context) = Create(new ForbiddenException());

        // Act
        var (statusCode, _) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(403, statusCode);
    }

    // ── ConflictException → 409 ───────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        // Arrange
        var (middleware, context) = Create(new ConflictException("Duplicate entry."));

        // Act
        var (statusCode, _) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(409, statusCode);
    }

    // ── TenantException → 400 ─────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_TenantException_Returns400()
    {
        // Arrange
        var (middleware, context) = Create(new TenantException("Bad tenant."));

        // Act
        var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(400, statusCode);
        Assert.Equal(ApiResponseCode.Error, body.RootElement.GetProperty("Code").GetInt32());
        Assert.Equal(ApiResponseType.Error, body.RootElement.GetProperty("Type").GetString());
    }

    // ── OperationCanceledException → 499 ──────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_Returns499()
    {
        // Arrange
        var (middleware, context) = Create(new OperationCanceledException());

        // Act
        var (statusCode, _) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(499, statusCode);
    }

    // ── Unknown exception → 500 ───────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnknownException_Returns500()
    {
        // Arrange
        var (middleware, context) = Create(new InvalidOperationException("Boom"));

        // Act
        var (statusCode, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.Equal(500, statusCode);
        Assert.Equal(ApiResponseCode.Error, body.RootElement.GetProperty("Code").GetInt32());
        Assert.Equal(ApiResponseType.Error, body.RootElement.GetProperty("Type").GetString());
        Assert.True(body.RootElement.TryGetProperty("Exception", out _));
        Assert.False(body.RootElement.TryGetProperty("Errors", out _));
    }

    // ── Response structure ────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ApiResponse_ContainsCodeTypeAndMessage()
    {
        // Arrange
        var (middleware, context) = Create(new NotFoundException("Order", "ORD-001"));

        // Act
        var (_, body) = await InvokeAndReadAsync(middleware, context);

        // Assert — base properties are always present
        Assert.True(body.RootElement.TryGetProperty("Code", out _));
        Assert.True(body.RootElement.TryGetProperty("Type", out _));
        Assert.True(body.RootElement.TryGetProperty("Message", out _));
        Assert.True(body.RootElement.TryGetProperty("Timestamp", out _));

        // Assert — subtype properties are absent for non-validation, non-500 errors
        Assert.False(body.RootElement.TryGetProperty("Errors", out _));
        Assert.False(body.RootElement.TryGetProperty("Exception", out _));
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_IncludesExceptionDetail()
    {
        // Arrange
        var (middleware, context) = Create(
            new InvalidOperationException("Boom"), CreateEnvironment("Development"));

        // Act
        var (_, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.True(body.RootElement.TryGetProperty("Exception", out var exDetail));
        Assert.Equal("System.InvalidOperationException", exDetail.GetProperty("Type").GetString());
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ExcludesExceptionDetail()
    {
        // Arrange
        var (middleware, context) = Create(
            new InvalidOperationException("Boom"), CreateEnvironment("Production"));

        // Act
        var (_, body) = await InvokeAndReadAsync(middleware, context);

        // Assert — exception property should be null / omitted
        Assert.False(body.RootElement.TryGetProperty("Exception", out _));
    }

    // ── RequestId stamping ────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ErrorResponse_IncludesRequestId()
    {
        // Arrange
        var (middleware, context) = Create(new InvalidOperationException("Boom"));
        context.TraceIdentifier = "test-trace-id-abc";

        // Act
        var (_, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.True(body.RootElement.TryGetProperty("RequestId", out var requestId));
        Assert.Equal("test-trace-id-abc", requestId.GetString());
    }

    [Fact]
    public async Task InvokeAsync_ValidationResponse_IncludesRequestId()
    {
        // Arrange
        var (middleware, context) = Create(new ValidationException("Name", "Required."));
        context.TraceIdentifier = "validation-trace-id";

        // Act
        var (_, body) = await InvokeAndReadAsync(middleware, context);

        // Assert
        Assert.True(body.RootElement.TryGetProperty("RequestId", out var requestId));
        Assert.Equal("validation-trace-id", requestId.GetString());
    }
}
