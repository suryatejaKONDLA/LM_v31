namespace CITL.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed JWT configuration bound from <c>appsettings.json</c> "Jwt" section.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Gets the Base64-encoded HMAC-SHA256 secret key.</summary>
    public required string SecretKey { get; init; }

    /// <summary>Gets the token issuer (iss claim).</summary>
    public required string Issuer { get; init; }

    /// <summary>Gets the token audience (aud claim).</summary>
    public required string Audience { get; init; }

    /// <summary>Gets the access token lifetime in minutes.</summary>
    public required int AccessTokenExpirationMinutes { get; init; }

    /// <summary>Gets the refresh token lifetime in days.</summary>
    public required int RefreshTokenExpirationDays { get; init; }

    /// <summary>Gets the clock skew tolerance in minutes.</summary>
    public int ClockSkewMinutes { get; init; } = 1;
}
