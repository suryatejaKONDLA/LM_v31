using System.Security.Claims;
using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using CITL.WebApi.Attributes;
using CITL.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.WebApi.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="TenantGuardMiddleware"/>.
/// Verifies JWT tenant_id claim cross-validation against request header tenant.
/// </summary>
public sealed class TenantGuardMiddlewareTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private bool _nextCalled;

    private TenantGuardMiddleware CreateMiddleware()
    {
        return new(
            _ => { _nextCalled = true; return Task.CompletedTask; },
            NullLogger<TenantGuardMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateAuthenticatedContext(string jwtTenantId)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var claims = new[] { new Claim(TenantConstants.JwtClaimType, jwtTenantId) };
        var identity = new ClaimsIdentity(claims, "test-scheme");
        context.User = new(identity);
        return context;
    }

    // ── Unauthenticated → passes through ─────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnauthenticatedRequest_CallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns("tn_a");

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.True(_nextCalled);
    }

    // ── Matching JWT claim → passes through ───────────────────────────────────

    [Fact]
    public async Task InvokeAsync_MatchingJwtClaim_CallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateAuthenticatedContext("tn_company_a");
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns("tn_company_a");

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.True(_nextCalled);
    }

    // ── Mismatching JWT claim → 403 ───────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_MismatchedJwtClaim_Returns403()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateAuthenticatedContext("tn_company_b");
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns("tn_company_a");

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.Equal(403, context.Response.StatusCode);
        Assert.False(_nextCalled);
    }

    // ── BypassTenant → skips guard ────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WithBypassTenantEndpoint_CallsNextRegardless()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateAuthenticatedContext("tn_wrong");
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns("tn_correct");
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new(new BypassTenantAttribute()),
            "bypass-test");
        context.SetEndpoint(endpoint);

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.True(_nextCalled);
    }

    // ── Tenant not resolved → passes through ─────────────────────────────────

    [Fact]
    public async Task InvokeAsync_TenantNotResolved_CallsNextWithoutValidation()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateAuthenticatedContext("tn_a");
        _tenantContext.IsResolved.Returns(false);

        // Act
        await middleware.InvokeAsync(context, _tenantContext);

        // Assert
        Assert.True(_nextCalled);
    }
}
