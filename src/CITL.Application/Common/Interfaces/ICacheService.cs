namespace CITL.Application.Common.Interfaces;

/// <summary>
/// L1/L2 cache abstraction with tenant-aware key management.
/// L1 = in-process MemoryCache, L2 = distributed Redis cache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache (L1 first, then L2).
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in both L1 and L2 cache.
    /// </summary>
    Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from both L1 and L2 cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a key exists in cache (L1 or L2).
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache, or creates it using the factory and stores it.
    /// Prevents cache stampede by using a simple lock per key.
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) where T : class;
}
