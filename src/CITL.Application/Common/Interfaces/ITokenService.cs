using CITL.SharedKernel.Results;

namespace CITL.Application.Common.Interfaces;

/// <summary>
/// JWT access/refresh token lifecycle management.
/// Defined in Application; implemented in Infrastructure with <c>System.IdentityModel.Tokens.Jwt</c>.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token containing user identity and tenant claims.
    /// The token carries identity only — roles and branches are not embedded.
    /// </summary>
    /// <param name="loginId">The user's primary key.</param>
    /// <param name="loginUser">The username.</param>
    /// <param name="loginName">The display name.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>The signed JWT access token string.</returns>
    string GenerateAccessToken(
        int loginId,
        string loginUser,
        string loginName,
        string tenantId);

    /// <summary>
    /// Generates a cryptographically random refresh token.
    /// </summary>
    /// <returns>A Base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Blacklists a JWT access token so it cannot be used again.
    /// Dual-writes to Redis (fast lookup) and DB (audit trail).
    /// </summary>
    /// <param name="token">The JWT access token to blacklist.</param>
    /// <param name="tenantId">The tenant identifier for key scoping.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task BlacklistTokenAsync(string token, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a JWT access token has been blacklisted.
    /// </summary>
    /// <param name="token">The JWT access token to check.</param>
    /// <param name="tenantId">The tenant identifier for key scoping.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the token is blacklisted; otherwise <c>false</c>.</returns>
    Task<bool> IsTokenBlacklistedAsync(string token, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a refresh token hash in both Redis and the database.
    /// </summary>
    /// <param name="loginUser">The username the token belongs to.</param>
    /// <param name="refreshToken">The raw refresh token string.</param>
    /// <param name="tenantId">The tenant identifier for key scoping.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task StoreRefreshTokenAsync(string loginUser, string refreshToken, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a refresh token against stored hash in Redis (or DB fallback).
    /// </summary>
    /// <param name="loginUser">The username the token belongs to.</param>
    /// <param name="refreshToken">The raw refresh token string to validate.</param>
    /// <param name="tenantId">The tenant identifier for key scoping.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure with an appropriate error.</returns>
    Task<Result> ValidateRefreshTokenAsync(string loginUser, string refreshToken, string tenantId, CancellationToken cancellationToken);
}
