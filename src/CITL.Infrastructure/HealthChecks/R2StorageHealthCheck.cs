using Amazon.S3;
using CITL.Application.Core.FileStorage;
using CITL.Infrastructure.Core.FileStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks Cloudflare R2 (S3-compatible) connectivity by listing bucket objects.
/// Only runs when the configured storage provider is "R2".
/// </summary>
internal sealed class R2StorageHealthCheck(
    IFileStorageProvider fileStorageProvider,
    IOptions<FileStorageSettings> options) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        // If not using R2, report as healthy (not applicable)
        if (!string.Equals(options.Value.Provider, "R2", StringComparison.OrdinalIgnoreCase))
        {
            data["Provider"] = options.Value.Provider;
            data["Reason"] = "R2 provider is not active; skipping check.";
            return HealthCheckResult.Healthy("R2 check skipped — not the active provider.", data);
        }

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Use the provider to verify connectivity: check if a non-existent file returns false
            var exists = await fileStorageProvider.ExistsAsync(
                $"__health_check_{Guid.NewGuid():N}",
                cancellationToken).ConfigureAwait(false);

            sw.Stop();

            data["ResponseTimeMs"] = sw.ElapsedMilliseconds;
            data["BucketName"] = options.Value.R2BucketName;

            return HealthCheckResult.Healthy("R2 storage is responsive.", data);
        }
        catch (AmazonS3Exception ex)
        {
            data["Error"] = ex.Message;
            data["StatusCode"] = (int)ex.StatusCode;
            return HealthCheckResult.Unhealthy("R2 storage is unreachable.", ex, data);
        }
        catch (Exception ex)
        {
            data["Error"] = ex.Message;
            return HealthCheckResult.Unhealthy("R2 storage is unreachable.", ex, data);
        }
    }
}
