namespace CITL.SharedKernel.Constants;

/// <summary>
/// Cross-cutting constants for authentication and authorization.
/// Used by middleware, infrastructure, and application layers.
/// </summary>
public static class AuthConstants
{
    /// <summary>JWT claim type for the user's login ID (primary key).</summary>
    public const string LoginIdClaimType = "login_id";

    /// <summary>JWT claim type for the user's login username.</summary>
    public const string LoginUserClaimType = "login_user";

    /// <summary>JWT claim type for the user's display name.</summary>
    public const string LoginNameClaimType = "login_name";

    /// <summary>Redis key prefix for blacklisted JWT access tokens.</summary>
    public const string BlacklistKeyPrefix = "blacklist";

    /// <summary>Redis key prefix for refresh token storage.</summary>
    public const string RefreshTokenKeyPrefix = "refresh";

    /// <summary>Redis key prefix for cached data.</summary>
    public const string CacheKeyPrefix = "cache";
}
