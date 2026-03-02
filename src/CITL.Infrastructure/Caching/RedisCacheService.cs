using System.Collections.Concurrent;
using System.Text.Json;
using CITL.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CITL.Infrastructure.Caching;

/// <summary>
/// Two-tier cache: L1 in-process <see cref="IMemoryCache"/> + L2 distributed <see cref="IDistributedCache"/> (Redis).
/// Tenant-aware key scoping is the caller's responsibility — this service operates on raw keys.
/// </summary>
/// <param name="memoryCache">The L1 in-process cache.</param>
/// <param name="distributedCache">The L2 distributed cache (Redis).</param>
/// <param name="logger">The logger.</param>
internal sealed partial class RedisCacheService(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new(StringComparer.Ordinal);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        // L1 check
        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            LogL1Hit(logger, key);
            return cached;
        }

        // L2 check
        try
        {
            var bytes = await distributedCache.GetAsync(key, cancellationToken).ConfigureAwait(false);

            if (bytes is not null && bytes.Length > 0)
            {
                var value = JsonSerializer.Deserialize<T>(bytes, JsonOptions);

                if (value is not null)
                {
                    LogL2Hit(logger, key);

                    // Promote to L1
                    memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                    return value;
                }
            }
        }
        catch (Exception ex)
        {
            LogRedisError(logger, nameof(GetAsync), key, ex);
        }

        LogCacheMiss(logger, key);
        return null;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, CacheEntryOptions? options, CancellationToken cancellationToken) where T : class
    {
        options ??= CacheEntryOptions.Default;

        // L1
        var l1Options = new MemoryCacheEntryOptions();

        if (options.AbsoluteExpiration.HasValue)
        {
            l1Options.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration;
        }

        if (options.SlidingExpiration.HasValue)
        {
            l1Options.SlidingExpiration = options.SlidingExpiration;
        }

        memoryCache.Set(key, value, l1Options);

        if (options.L1Only)
        {
            return;
        }

        // L2
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

            var l2Options = new DistributedCacheEntryOptions();

            if (options.AbsoluteExpiration.HasValue)
            {
                l2Options.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration;
            }

            if (options.SlidingExpiration.HasValue)
            {
                l2Options.SlidingExpiration = options.SlidingExpiration;
            }

            await distributedCache.SetAsync(key, bytes, l2Options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisError(logger, nameof(SetAsync), key, ex);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        memoryCache.Remove(key);

        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisError(logger, nameof(RemoveAsync), key, ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue(key, out _))
        {
            return true;
        }

        try
        {
            var bytes = await distributedCache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            return bytes is not null && bytes.Length > 0;
        }
        catch (Exception ex)
        {
            LogRedisError(logger, nameof(ExistsAsync), key, ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions? options,
        CancellationToken cancellationToken) where T : class
    {
        // Try cache first
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);

        if (cached is not null)
        {
            return cached;
        }

        // Acquire per-key lock to prevent cache stampede
        var semaphore = Locks.GetOrAdd(key, static _ => new(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Double-check after acquiring lock
            cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);

            if (cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken).ConfigureAwait(false);
            await SetAsync(key, value, options, cancellationToken).ConfigureAwait(false);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache L1 hit — Key: {Key}")]
    private static partial void LogL1Hit(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache L2 hit (Redis) — Key: {Key}")]
    private static partial void LogL2Hit(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache miss — Key: {Key}")]
    private static partial void LogCacheMiss(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis {Operation} error — Key: {Key}")]
    private static partial void LogRedisError(ILogger logger, string operation, string key, Exception ex);
}
