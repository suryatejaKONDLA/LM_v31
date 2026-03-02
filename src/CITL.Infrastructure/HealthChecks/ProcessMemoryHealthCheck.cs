using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks the current process memory (working set) against a configurable threshold.
/// </summary>
internal sealed class ProcessMemoryHealthCheck(
    IOptions<ProcessMemoryHealthCheckSettings> options) : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var thresholdBytes = settings.ThresholdMB * 1_048_576L;
        var degradedBytes = (long)(thresholdBytes * 0.8);

        using var process = Process.GetCurrentProcess();
        var usedBytes = process.WorkingSet64;
        var usedMB = Math.Round(usedBytes / 1_048_576.0, 2);
        var usedPercent = thresholdBytes > 0
            ? Math.Round((double)usedBytes / thresholdBytes * 100.0, 2)
            : 0.0;

        var data = new Dictionary<string, object>
        {
            ["UsedMB"] = usedMB,
            ["ThresholdMB"] = settings.ThresholdMB,
            ["UsedPercent"] = usedPercent
        };

        if (usedBytes >= thresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Process memory critically high: {usedMB} MB ({usedPercent:F1}% of {settings.ThresholdMB} MB threshold).",
                data: data));
        }

        if (usedBytes >= degradedBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Process memory elevated: {usedMB} MB ({usedPercent:F1}% of {settings.ThresholdMB} MB threshold).",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Process memory OK: {usedMB} MB ({usedPercent:F1}% of {settings.ThresholdMB} MB threshold).",
            data));
    }
}

/// <summary>
/// Settings for <see cref="ProcessMemoryHealthCheck"/>.
/// </summary>
public sealed class ProcessMemoryHealthCheckSettings
{
    /// <summary>Configuration section name in appsettings.json.</summary>
    public const string SectionName = "HealthChecks:ProcessMemory";

    /// <summary>Memory threshold in megabytes. Defaults to 1024 MB (1 GB).</summary>
    public long ThresholdMB { get; init; } = 1024;
}
