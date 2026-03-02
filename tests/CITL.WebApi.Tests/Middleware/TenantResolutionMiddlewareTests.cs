using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using CITL.WebApi.Attributes;
using CITL.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.WebApi.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="TenantResolutionMiddleware"/>.
/// Verifies header parsing, registry lookup, bypass, and error responses.
/// </summary>
public sealed class TenantResolutionMiddlewareTests
{
    private readonly ITenantRegistry _registry = Substitute.For<ITenantRegistry>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private bool _nextCalled;

    private TenantResolutionMiddleware CreateMiddleware()
    {
        return new(
            _ => { _nextCalled = true; return Task.CompletedTask; },
            _registry,
            NullLogger<TenantResolutionMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateContext(string? tenantId = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (tenantId is not null)
        {
            context.Request.Headers[TenantConstants.HeaderName] = tenantId;
        }

        return context;
    }

    // ── Missing header → 400 ──────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_MissingTenantHeader_Returns400()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext();

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_EmptyTenantHeader_Returns400()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext("");

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    // ── Unknown tenant → 400 ──────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnknownTenant_Returns400()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext("unknown_tenant");
        _registry.TryGetDatabaseName("unknown_tenant", out Arg.Any<string?>()).Returns(false);

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    // ── Valid tenant → sets context & calls next ──────────────────────────────

    [Fact]
    public async Task InvokeAsync_ValidTenant_SetsTenantContextAndCallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext("tn_company_a");
        _registry
            .TryGetDatabaseName("tn_company_a", out Arg.Any<string?>())
            .Returns(x => { x[1] = "CITL_CompanyA"; return true; });

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        _tenantContext.Received(1).SetTenant("tn_company_a", "CITL_CompanyA");
        Assert.True(_nextCalled);
    }

    // ── BypassTenant → skips resolution ───────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WithBypassTenantEndpoint_SkipsResolutionAndCallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext(); // no header
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new(new BypassTenantAttribute()),
            "bypass-test");
        context.SetEndpoint(endpoint);

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.True(_nextCalled);
        _tenantContext.DidNotReceive().SetTenant(Arg.Any<string>(), Arg.Any<string>());
    }
}
