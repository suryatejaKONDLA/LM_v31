namespace CITL.WebApi.Attributes;

/// <summary>
/// Marks an endpoint or controller to bypass tenant resolution and tenant guard middleware.
/// Apply to endpoints that do not require a tenant context such as health checks,
/// authentication endpoints, or public resources.
/// </summary>
/// <remarks>
/// Uses ASP.NET Core endpoint metadata — works with both controllers and minimal APIs.
/// <para>
/// For controllers: <c>[BypassTenant]</c> on action or controller class.
/// </para>
/// <para>
/// For minimal APIs: <c>app.MapGet("/path", handler).BypassTenant();</c>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class BypassTenantAttribute : Attribute;

/// <summary>
/// Extension methods for applying tenant-related endpoint metadata to minimal APIs.
/// </summary>
public static class TenantEndpointExtensions
{
    /// <summary>
    /// Marks the endpoint to bypass tenant resolution and guard middleware.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder BypassTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new BypassTenantAttribute());
        return builder;
    }
}
