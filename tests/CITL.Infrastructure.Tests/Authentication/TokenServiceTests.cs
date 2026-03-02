using System.IdentityModel.Tokens.Jwt;
using CITL.Application.Core.Authentication;
using CITL.Infrastructure.Authentication;
using CITL.SharedKernel.Constants;
using CITL.SharedKernel.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CITL.Infrastructure.Tests.Authentication;

/// <summary>
/// Unit tests for <see cref="TokenService"/>.
/// Covers JWT generation, refresh token lifecycle, and blacklisting.
/// </summary>
public sealed class TokenServiceTests
{
    private const string TenantId = "CITLPOS";
    private const string TestUser = "admin";
    private const string TestLoginName = "Admin User";
    private const int TestLoginId = 42;

    private static readonly JwtSettings _defaultSettings = new()
    {
        SecretKey = Convert.ToBase64String(new byte[64]),
        Issuer = "CITL-Test",
        Audience = "CITL-Test-Client",
        AccessTokenExpirationMinutes = 30,
        RefreshTokenExpirationDays = 7,
        ClockSkewMinutes = 1,
    };

    private readonly IDistributedCache _cache;
    private readonly IAuthenticationRepository _authRepository;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _cache = Substitute.For<IDistributedCache>();
        _authRepository = Substitute.For<IAuthenticationRepository>();

        _sut = new TokenService(
            Options.Create(_defaultSettings),
            _cache,
            _authRepository,
            NullLogger<TokenService>.Instance);
    }

    // ── GenerateAccessToken ───────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyJwtString()
    {
        // Act
        var token = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Contains('.', token);
    }

    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaims()
    {
        // Act
        var tokenString = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal(TestLoginId.ToString(System.Globalization.CultureInfo.InvariantCulture), token.Claims.First(c => c.Type == AuthConstants.LoginIdClaimType).Value);
        Assert.Equal(TestUser, token.Claims.First(c => c.Type == AuthConstants.LoginUserClaimType).Value);
        Assert.Equal(TestLoginName, token.Claims.First(c => c.Type == AuthConstants.LoginNameClaimType).Value);
        Assert.Equal(TenantId, token.Claims.First(c => c.Type == TenantConstants.JwtClaimType).Value);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        // Act
        var tokenString = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        Assert.Equal(_defaultSettings.Issuer, token.Issuer);
        Assert.Contains(_defaultSettings.Audience, token.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAfterConfiguredMinutes()
    {
        // Act
        var tokenString = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
        var expectedExpiry = DateTime.UtcNow.AddMinutes(_defaultSettings.AccessTokenExpirationMinutes);

        // Allow 5-second tolerance for test execution time
        Assert.InRange(token.ValidTo, expectedExpiry.AddSeconds(-5), expectedExpiry.AddSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_HasUniqueJti()
    {
        // Act
        var token1 = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);
        var token2 = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GenerateAccessToken_CanBeValidatedWithSameKey()
    {
        // Act
        var tokenString = _sut.GenerateAccessToken(TestLoginId, TestUser, TestLoginName, TenantId);

        // Assert — validate with the same key
        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _defaultSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _defaultSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_defaultSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(_defaultSettings.ClockSkewMinutes),
        };

        var principal = handler.ValidateToken(tokenString, validationParams, out var validatedToken);
        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
    }

    // ── GenerateRefreshToken ──────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        var decoded = Convert.FromBase64String(token);
        Assert.True(decoded.Length > 0);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    // ── BlacklistTokenAsync ───────────────────────────────────────────────

    [Fact]
    public async Task BlacklistTokenAsync_WritesToRedis()
    {
        // Act
        await _sut.BlacklistTokenAsync("test-token", TenantId, CancellationToken.None);

        // Assert
        await _cache.Received(1).SetAsync(
            Arg.Is<string>(key => key.StartsWith($"{AuthConstants.BlacklistKeyPrefix}:{TenantId}:")),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BlacklistTokenAsync_WritesToDatabase()
    {
        // Act
        await _sut.BlacklistTokenAsync("test-token", TenantId, CancellationToken.None);

        // Assert
        await _authRepository.Received(1)
            .BlacklistTokenAsync(Arg.Any<byte[]>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BlacklistTokenAsync_RedisFailure_StillWritesToDb()
    {
        // Arrange
        _cache.SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis down"));

        // Act — should not throw
        await _sut.BlacklistTokenAsync("test-token", TenantId, CancellationToken.None);

        // Assert — DB write still happens
        await _authRepository.Received(1)
            .BlacklistTokenAsync(Arg.Any<byte[]>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BlacklistTokenAsync_DbFailure_DoesNotThrow()
    {
        // Arrange
        _authRepository.BlacklistTokenAsync(Arg.Any<byte[]>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB down"));

        // Act & Assert — should not throw
        await _sut.BlacklistTokenAsync("test-token", TenantId, CancellationToken.None);
    }

    // ── IsTokenBlacklistedAsync ───────────────────────────────────────────

    [Fact]
    public async Task IsTokenBlacklistedAsync_TokenInRedis_ReturnsTrue()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("1"u8.ToArray());

        // Act
        var result = await _sut.IsTokenBlacklistedAsync("blacklisted-token", TenantId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTokenBlacklistedAsync_TokenNotInRedis_ReturnsFalse()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result = await _sut.IsTokenBlacklistedAsync("valid-token", TenantId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsTokenBlacklistedAsync_RedisFailure_ReturnsFalse()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis down"));

        // Act
        var result = await _sut.IsTokenBlacklistedAsync("token", TenantId, CancellationToken.None);

        // Assert — falls through, returns false (no DB fallback for blacklist)
        Assert.False(result);
    }

    // ── StoreRefreshTokenAsync ────────────────────────────────────────────

    [Fact]
    public async Task StoreRefreshTokenAsync_WritesToRedisAndDb()
    {
        // Act
        await _sut.StoreRefreshTokenAsync(TestUser, "refresh-token", TenantId, CancellationToken.None);

        // Assert
        await _cache.Received(1).SetAsync(
            Arg.Is<string>(key => key.Contains(TestUser) && key.Contains(TenantId)),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());

        await _authRepository.Received(1)
            .StoreRefreshTokenAsync(TestUser, Arg.Any<byte[]>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StoreRefreshTokenAsync_RedisFailure_StillWritesToDb()
    {
        // Arrange
        _cache.SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis down"));

        // Act
        await _sut.StoreRefreshTokenAsync(TestUser, "refresh-token", TenantId, CancellationToken.None);

        // Assert
        await _authRepository.Received(1)
            .StoreRefreshTokenAsync(TestUser, Arg.Any<byte[]>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    // ── ValidateRefreshTokenAsync ─────────────────────────────────────────

    [Fact]
    public async Task ValidateRefreshTokenAsync_MatchingHashInRedis_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var expectedHash = CryptoHelper.ComputeSha256(refreshToken);

        _cache.GetAsync(
                Arg.Is<string>(key => key.Contains(TestUser)),
                Arg.Any<CancellationToken>())
            .Returns(expectedHash);

        // Act
        var result = await _sut.ValidateRefreshTokenAsync(TestUser, refreshToken, TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_MismatchedHashInRedis_ReturnsFailure()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]); // Wrong hash

        // Act
        var result = await _sut.ValidateRefreshTokenAsync(TestUser, "some-token", TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.RefreshToken", result.Error.Code);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_NotInRedis_FallsBackToDb_Valid()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        _authRepository
            .ValidateRefreshTokenAsync(TestUser, Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.ValidateRefreshTokenAsync(TestUser, "token", TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_NotInRedis_FallsBackToDb_Invalid()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        _authRepository
            .ValidateRefreshTokenAsync(TestUser, Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.ValidateRefreshTokenAsync(TestUser, "expired-token", TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_RedisFailure_FallsBackToDb()
    {
        // Arrange
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Redis down"));

        _authRepository
            .ValidateRefreshTokenAsync(TestUser, Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.ValidateRefreshTokenAsync(TestUser, "token", TenantId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
