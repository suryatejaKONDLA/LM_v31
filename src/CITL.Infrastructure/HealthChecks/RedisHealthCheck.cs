using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CITL.Infrastructure.HealthChecks;

/// <summary>
/// Checks Redis connectivity by performing a SET + GET round-trip.
/// Reports response latency in <see cref="HealthCheckResult.Data"/>.
/// </summary>
internal sealed class RedisHealthCheck(IDistributedCache distributedCache) : IHealthCheck
{
    private const string HealthKey = "health:ping";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var testValue = System.Text.Encoding.UTF8.GetBytes("pong");
            await distributedCache.SetAsync(
                HealthKey,
                testValue,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) },
                cancellationToken).ConfigureAwait(false);

            var result = await distributedCache.GetAsync(HealthKey, cancellationToken).ConfigureAwait(false);

            sw.Stop();

            if (result is null || result.Length == 0)
            {
                data["ResponseTimeMs"] = sw.ElapsedMilliseconds;
                return HealthCheckResult.Unhealthy("Redis SET succeeded but GET returned null.", data: data);
            }

            data["ResponseTimeMs"] = sw.ElapsedMilliseconds;
            return HealthCheckResult.Healthy("Redis is responsive.", data);
        }
        catch (Exception ex)
        {
            data["Error"] = ex.Message;
            return HealthCheckResult.Unhealthy("Redis is unreachable.", ex, data);
        }
    }
}
