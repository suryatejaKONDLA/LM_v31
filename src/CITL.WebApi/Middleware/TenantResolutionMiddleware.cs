using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using CITL.WebApi.Attributes;
using CITL.WebApi.Responses;

namespace CITL.WebApi.Middleware;

/// <summary>
/// Resolves the current tenant from the <c>X-Tenant-Id</c> request header,
/// looks up the database name via <see cref="ITenantRegistry"/>,
/// and sets the scoped <see cref="ITenantContext"/>.
/// </summary>
/// <remarks>
/// Runs <b>before</b> authentication so that login and other pre-auth endpoints
/// have tenant context available (e.g., to query the correct tenant database).
/// Endpoints marked with <see cref="BypassTenantAttribute"/> are skipped entirely.
/// <para>
/// <b>Pipeline order</b>: GlobalException → <b>TenantResolution</b> → Auth → TenantGuard
/// </para>
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="tenantRegistry">The tenant registry for identifier-to-database resolution.</param>
/// <param name="logger">The logger.</param>
public sealed partial class TenantResolutionMiddleware(
    RequestDelegate next,
    ITenantRegistry tenantRegistry,
    ILogger<TenantResolutionMiddleware> logger)
{

    /// <summary>
    /// Reads the <c>X-Tenant-Id</c> header, resolves the database name from
    /// <see cref="ITenantRegistry"/>, and populates the scoped <see cref="ITenantContext"/>.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantContext">The scoped tenant context to populate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Endpoints marked with [BypassTenant] skip resolution entirely
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<BypassTenantAttribute>() is not null)
        {
            await next(context);
            return;
        }

        // Read the opaque tenant identifier from the request header
        if (!context.Request.Headers.TryGetValue(TenantConstants.HeaderName, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.ToString()))
        {
            LogMissingTenantHeader(logger, TenantConstants.HeaderName);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Error($"The '{TenantConstants.HeaderName}' header is required."));
            return;
        }

        var tenantId = headerValues.ToString();

        // Resolve the database name from the tenant registry (FrozenDictionary — O(1))
        if (!tenantRegistry.TryGetDatabaseName(tenantId, out var databaseName))
        {
            LogUnknownTenant(logger, tenantId);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Error("Invalid tenant identifier."));
            return;
        }

        // Set the scoped tenant context for all downstream services and repositories
        tenantContext.SetTenant(tenantId, databaseName);

        LogTenantResolved(logger, tenantId);

        // Push TenantId into the structured-logging scope so every downstream
        // log entry includes it (appears in console and all sinks)
        using (logger.BeginScope(
            new KeyValuePair<string, object>("TenantId", tenantId)))
        {
            await next(context);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Missing required tenant header: {HeaderName}")]
    private static partial void LogMissingTenantHeader(ILogger logger, string headerName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown tenant identifier: {TenantId}")]
    private static partial void LogUnknownTenant(ILogger logger, string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tenant resolved: {TenantId}")]
    private static partial void LogTenantResolved(ILogger logger, string tenantId);
}
