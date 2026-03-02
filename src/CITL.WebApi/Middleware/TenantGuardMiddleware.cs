using System.Security.Claims;
using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using CITL.WebApi.Attributes;
using CITL.WebApi.Responses;

namespace CITL.WebApi.Middleware;

/// <summary>
/// Runs <b>after</b> authentication to cross-validate the JWT <c>tenant_id</c> claim
/// against the <c>X-Tenant-Id</c> header value resolved by
/// <see cref="TenantResolutionMiddleware"/>.
/// </summary>
/// <remarks>
/// Prevents tenant spoofing: a user authenticated for tenant A cannot access
/// tenant B by changing the request header. Skips validation for unauthenticated
/// requests (handled by auth middleware) and endpoints marked with
/// <see cref="BypassTenantAttribute"/>.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger.</param>
public sealed partial class TenantGuardMiddleware(
    RequestDelegate next,
    ILogger<TenantGuardMiddleware> logger)
{

    /// <summary>
    /// Invokes the middleware. For authenticated requests, verifies the JWT
    /// <c>tenant_id</c> claim matches the resolved tenant from the header.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantContext">The scoped tenant context populated by tenant resolution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Endpoints marked with [BypassTenant] skip guard entirely
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<BypassTenantAttribute>() is not null)
        {
            await next(context);
            return;
        }

        // Cross-validate only for authenticated requests with a resolved tenant
        if (context.User.Identity?.IsAuthenticated is true && tenantContext.IsResolved)
        {
            var claimTenantId = context.User.FindFirstValue(TenantConstants.JwtClaimType);

            if (!string.Equals(claimTenantId, tenantContext.TenantId, StringComparison.Ordinal))
            {
                LogTenantMismatch(logger, tenantContext.TenantId, claimTenantId);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Error("The authenticated user is not authorized for the requested tenant."));
                return;
            }
        }

        await next(context);
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Tenant mismatch — header: {HeaderTenantId}, JWT claim: {ClaimTenantId}")]
    private static partial void LogTenantMismatch(
        ILogger logger, string headerTenantId, string? claimTenantId);
}
