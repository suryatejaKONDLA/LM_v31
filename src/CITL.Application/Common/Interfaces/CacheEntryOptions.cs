namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Options for cache entry expiration.
/// </summary>
public sealed class CacheEntryOptions
{
    /// <summary>Absolute expiration relative to now.</summary>
    public TimeSpan? AbsoluteExpiration { get; init; }

    /// <summary>Sliding expiration — resets on each access.</summary>
    public TimeSpan? SlidingExpiration { get; init; }

    /// <summary>When true, only store in L1 (MemoryCache), skip Redis.</summary>
    public bool L1Only { get; init; }

    /// <summary>Default options: 5 minutes absolute expiration.</summary>
    public static CacheEntryOptions Default => new() { AbsoluteExpiration = TimeSpan.FromMinutes(5) };

    /// <summary>Short-lived: 1 minute absolute expiration.</summary>
    public static CacheEntryOptions ShortLived => new() { AbsoluteExpiration = TimeSpan.FromMinutes(1) };

    /// <summary>Long-lived: 30 minutes absolute expiration.</summary>
    public static CacheEntryOptions LongLived => new() { AbsoluteExpiration = TimeSpan.FromMinutes(30) };
}
