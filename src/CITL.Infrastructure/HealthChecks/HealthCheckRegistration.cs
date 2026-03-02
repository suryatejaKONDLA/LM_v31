using CITL.Infrastructure.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace CITL.Infrastructure;

/// <summary>
/// Registers Infrastructure health checks into the ASP.NET Core health checks system.
/// </summary>
public static class HealthCheckRegistration
{
    /// <summary>
    /// Adds all Infrastructure-layer health checks.
    /// </summary>
    public static IHealthChecksBuilder AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        // HttpClientFactory is needed by Grafana and OTLP health checks
        services.AddHttpClient();

        return services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>(
                "SqlServer",
                tags: ["database", "critical"])
            .AddCheck<RedisHealthCheck>(
                "Redis",
                tags: ["cache", "critical"])
            .AddCheck<R2StorageHealthCheck>(
                "R2Storage",
                tags: ["storage"])
            .AddCheck<DiskSpaceHealthCheck>(
                "DiskSpace",
                tags: ["storage"])
            .AddCheck<QuartzSchedulerHealthCheck>(
                "Quartz",
                tags: ["scheduler"])
            .AddCheck<GrafanaHealthCheck>(
                "Grafana",
                tags: ["observability"])
            .AddCheck<OtlpCollectorHealthCheck>(
                "OtlpCollector",
                tags: ["observability"])
            .AddCheck<MailHealthCheck>(
                "Mail",
                tags: ["email"])
            .AddCheck<ProcessMemoryHealthCheck>(
                "ProcessMemory",
                tags: ["system"]);
    }
}
