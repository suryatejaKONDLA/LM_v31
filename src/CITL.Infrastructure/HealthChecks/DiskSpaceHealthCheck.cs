using CITL.Infrastructure.Core.FileStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks storage usage of the configured local folder against its quota.
/// Enumerates files in <see cref="FileStorageSettings.LocalBasePath"/> and compares
/// total size to <see cref="FileStorageSettings.LocalQuotaGB"/>.
/// </summary>
internal sealed class DiskSpaceHealthCheck(
    IOptions<FileStorageSettings> options) : IHealthCheck
{
    private const double DegradedThresholdPercent = 80.0;
    private const double UnhealthyThresholdPercent = 95.0;

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var basePath = settings.LocalBasePath;
        var quotaGB = settings.LocalQuotaGB;
        var data = new Dictionary<string, object>();

        try
        {
            var fullPath = Path.GetFullPath(basePath);
            data["BasePath"] = fullPath;
            data["QuotaGB"] = quotaGB;

            if (!Directory.Exists(fullPath))
            {
                data["Error"] = "Storage directory does not exist.";
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Storage directory not found: {fullPath}", data: data));
            }

            var usedBytes = CalculateDirectorySize(fullPath);
            var quotaBytes = quotaGB * 1_073_741_824.0;
            var usedGB = Math.Round(usedBytes / 1_073_741_824.0, 2);
            var freeGB = Math.Round(Math.Max(0, quotaGB - usedGB), 2);
            var usedPercent = quotaBytes > 0 ? usedBytes / quotaBytes * 100.0 : 0.0;
            var freePercent = Math.Round(Math.Max(0, 100.0 - usedPercent), 2);

            data["UsedGB"] = usedGB;
            data["FreeGB"] = freeGB;
            data["UsedPercent"] = Math.Round(usedPercent, 2);
            data["FreePercent"] = freePercent;

            if (usedPercent >= UnhealthyThresholdPercent)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Storage quota critically full: {usedPercent:F1}% used ({usedGB} / {quotaGB} GB).", data: data));
            }

            if (usedPercent >= DegradedThresholdPercent)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Storage quota running high: {usedPercent:F1}% used ({usedGB} / {quotaGB} GB).", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"Storage usage OK: {usedPercent:F1}% used ({usedGB} / {quotaGB} GB). {freeGB} GB free.", data));
        }
        catch (Exception ex)
        {
            data["Error"] = ex.Message;
            return Task.FromResult(HealthCheckResult.Unhealthy("Unable to check storage usage.", ex, data));
        }
    }

    private static long CalculateDirectorySize(string path)
    {
        var size = 0L;

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            try
            {
                size += new FileInfo(file).Length;
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files we can't access
            }
            catch (FileNotFoundException)
            {
                // File may have been deleted between enumeration and size query
            }
        }

        return size;
    }
}
