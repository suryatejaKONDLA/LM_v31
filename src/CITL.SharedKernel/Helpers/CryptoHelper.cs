using System.Security.Cryptography;
using System.Text;

namespace CITL.SharedKernel.Helpers;

/// <summary>
/// Cryptographically secure token generation and hashing utilities.
/// All methods are thread-safe and allocation-optimized.
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// Generates a URL-safe Base64 token (RFC 4648) from cryptographically random bytes.
    /// Output uses <c>-</c> instead of <c>+</c>, <c>_</c> instead of <c>/</c>, no padding.
    /// </summary>
    /// <param name="byteCount">Number of random bytes (output length ≈ byteCount × 4/3).</param>
    public static string GenerateBase64UrlToken(int byteCount)
    {
        Span<byte> buffer = byteCount <= 128
            ? stackalloc byte[byteCount]
            : new byte[byteCount];

        RandomNumberGenerator.Fill(buffer);

        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Generates a standard Base64 token from cryptographically random bytes.
    /// </summary>
    /// <param name="byteCount">Number of random bytes.</param>
    public static string GenerateBase64Token(int byteCount)
    {
        Span<byte> buffer = byteCount <= 128
            ? stackalloc byte[byteCount]
            : new byte[byteCount];

        RandomNumberGenerator.Fill(buffer);

        return Convert.ToBase64String(buffer);
    }

    /// <summary>
    /// Generates a random code of the specified length using only the allowed characters.
    /// Uses cryptographic randomness — suitable for CAPTCHAs and verification codes.
    /// </summary>
    /// <param name="length">The number of characters in the output code.</param>
    /// <param name="allowedChars">The character set to pick from.</param>
    public static string GenerateRandomCode(int length, ReadOnlySpan<char> allowedChars)
    {
        Span<byte> randomBytes = length <= 256
            ? stackalloc byte[length]
            : new byte[length];

        RandomNumberGenerator.Fill(randomBytes);

        var result = new char[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = allowedChars[randomBytes[i] % allowedChars.Length];
        }

        return new string(result);
    }

    /// <summary>
    /// Computes the SHA-256 hash of a UTF-8 encoded string.
    /// </summary>
    public static byte[] ComputeSha256(string input)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(input));
    }
}
