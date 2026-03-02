using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Authentication;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Helpers;
using CITL.SharedKernel.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CITL.Infrastructure.Authentication;

/// <summary>
/// JWT access/refresh token lifecycle manager.
/// Dual-writes blacklist and refresh tokens to both Redis and the database.
/// </summary>
/// <param name="jwtSettings">The JWT configuration.</param>
/// <param name="distributedCache">The Redis distributed cache for fast token lookups.</param>
/// <param name="authRepository">The repository for DB persistence of token data.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class TokenService(
    IOptions<JwtSettings> jwtSettings,
    IDistributedCache distributedCache,
    IAuthenticationRepository authRepository,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    /// <inheritdoc />
    public string GenerateAccessToken(
        int loginId,
        string loginUser,
        string loginName,
        string tenantId)
    {
        var keyBytes = Convert.FromBase64String(_settings.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(AuthConstants.LoginIdClaimType, loginId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(AuthConstants.LoginUserClaimType, loginUser),
            new(AuthConstants.LoginNameClaimType, loginName),
            new(TenantConstants.JwtClaimType, tenantId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        return CryptoHelper.GenerateBase64Token(64);
    }

    /// <inheritdoc />
    public async Task BlacklistTokenAsync(string token, string tenantId, CancellationToken cancellationToken)
    {
        var tokenHash = CryptoHelper.ComputeSha256(token);
        var redisKey = $"{AuthConstants.BlacklistKeyPrefix}:{tenantId}:{Convert.ToBase64String(tokenHash)}";

        // Redis — fast path for validation checks
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.AccessTokenExpirationMinutes + _settings.ClockSkewMinutes)
            };

            await distributedCache.SetStringAsync(redisKey, "1", options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisError(logger, "BlacklistToken", ex);
        }

        // DB — audit trail via citlsp.BlackList_Token
        try
        {
            await authRepository.BlacklistTokenAsync(tokenHash, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogDbError(logger, "BlacklistToken", ex);
        }

        LogTokenBlacklisted(logger, tenantId);
    }

    /// <inheritdoc />
    public async Task<bool> IsTokenBlacklistedAsync(string token, string tenantId, CancellationToken cancellationToken)
    {
        var tokenHash = CryptoHelper.ComputeSha256(token);
        var redisKey = $"{AuthConstants.BlacklistKeyPrefix}:{tenantId}:{Convert.ToBase64String(tokenHash)}";

        // Redis fast path
        try
        {
            var value = await distributedCache.GetStringAsync(redisKey, cancellationToken).ConfigureAwait(false);

            if (value is not null)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            LogRedisError(logger, "IsTokenBlacklisted", ex);
            // Fall through to DB check
        }

        return false;
    }

    /// <inheritdoc />
    public async Task StoreRefreshTokenAsync(string loginUser, string refreshToken, string tenantId, CancellationToken cancellationToken)
    {
        var tokenHash = CryptoHelper.ComputeSha256(refreshToken);
        var expiryDate = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        var redisKey = $"{AuthConstants.RefreshTokenKeyPrefix}:{tenantId}:{loginUser}";

        // Redis — fast validation path
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_settings.RefreshTokenExpirationDays)
            };

            await distributedCache.SetAsync(redisKey, tokenHash, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisError(logger, "StoreRefreshToken", ex);
        }

        // DB — persistence via citlsp.Refresh_Token
        try
        {
            await authRepository.StoreRefreshTokenAsync(loginUser, tokenHash, expiryDate, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogDbError(logger, "StoreRefreshToken", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> ValidateRefreshTokenAsync(string loginUser, string refreshToken, string tenantId, CancellationToken cancellationToken)
    {
        var tokenHash = CryptoHelper.ComputeSha256(refreshToken);
        var redisKey = $"{AuthConstants.RefreshTokenKeyPrefix}:{tenantId}:{loginUser}";

        // Redis fast path
        try
        {
            var storedHash = await distributedCache.GetAsync(redisKey, cancellationToken).ConfigureAwait(false);

            if (storedHash is not null && storedHash.AsSpan().SequenceEqual(tokenHash))
            {
                return Result.Success();
            }

            if (storedHash is not null)
            {
                return Result.Failure(Error.Validation("RefreshToken", "Invalid refresh token."));
            }
        }
        catch (Exception ex)
        {
            LogRedisError(logger, "ValidateRefreshToken", ex);
            // Fall through to DB
        }

        // DB fallback
        var isValid = await authRepository.ValidateRefreshTokenAsync(loginUser, tokenHash, cancellationToken).ConfigureAwait(false);

        return isValid
            ? Result.Success()
            : Result.Failure(Error.Validation("RefreshToken", "Invalid or expired refresh token."));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis error during {Operation}")]
    private static partial void LogRedisError(ILogger logger, string operation, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Database error during {Operation}")]
    private static partial void LogDbError(ILogger logger, string operation, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Access token blacklisted for tenant {TenantId}")]
    private static partial void LogTokenBlacklisted(ILogger logger, string tenantId);
}
