using CITL.SharedKernel.Helpers;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="CryptoHelper"/>.
/// Validates token generation, randomness, format, and hashing correctness.
/// </summary>
public sealed class CryptoHelperTests
{
    // ── GenerateBase64UrlToken ─────────────────────────────────────────────────

    [Fact]
    public void GenerateBase64UrlToken_ReturnsNonEmptyString()
    {
        // Act
        var token = CryptoHelper.GenerateBase64UrlToken(32);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(32)]
    [InlineData(48)]
    [InlineData(64)]
    public void GenerateBase64UrlToken_ContainsNoUnsafeCharacters(int byteCount)
    {
        // Act
        var token = CryptoHelper.GenerateBase64UrlToken(byteCount);

        // Assert — RFC 4648 Base64Url: no +, /, or =
        Assert.DoesNotContain("+", token, StringComparison.Ordinal);
        Assert.DoesNotContain("/", token, StringComparison.Ordinal);
        Assert.DoesNotContain("=", token, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateBase64UrlToken_ProducesUniqueTokens()
    {
        // Act
        var token1 = CryptoHelper.GenerateBase64UrlToken(32);
        var token2 = CryptoHelper.GenerateBase64UrlToken(32);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateBase64UrlToken_LargeByteCount_DoesNotThrow()
    {
        // Act — tests the heap-alloc path (> 128 bytes)
        var token = CryptoHelper.GenerateBase64UrlToken(256);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    // ── GenerateBase64Token ───────────────────────────────────────────────────

    [Fact]
    public void GenerateBase64Token_ReturnsValidBase64()
    {
        // Act
        var token = CryptoHelper.GenerateBase64Token(64);

        // Assert — must be valid Base64 (may contain +, /, =)
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateBase64Token_ProducesUniqueTokens()
    {
        // Act
        var token1 = CryptoHelper.GenerateBase64Token(64);
        var token2 = CryptoHelper.GenerateBase64Token(64);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateBase64Token_LargeByteCount_DoesNotThrow()
    {
        // Act
        var token = CryptoHelper.GenerateBase64Token(256);

        // Assert
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(256, bytes.Length);
    }

    // ── GenerateRandomCode ────────────────────────────────────────────────────

    [Fact]
    public void GenerateRandomCode_ReturnsCorrectLength()
    {
        // Arrange
        const string chars = "ABCDEF123456";

        // Act
        var code = CryptoHelper.GenerateRandomCode(6, chars);

        // Assert
        Assert.Equal(6, code.Length);
    }

    [Fact]
    public void GenerateRandomCode_OnlyContainsAllowedCharacters()
    {
        // Arrange
        const string allowed = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";

        // Act
        var code = CryptoHelper.GenerateRandomCode(100, allowed);

        // Assert — every character must be in the allowed set
        foreach (var c in code)
        {
            Assert.Contains(c, allowed);
        }
    }

    [Fact]
    public void GenerateRandomCode_ProducesUniqueCodes()
    {
        // Arrange
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Act
        var code1 = CryptoHelper.GenerateRandomCode(16, chars);
        var code2 = CryptoHelper.GenerateRandomCode(16, chars);

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void GenerateRandomCode_LargeLength_DoesNotThrow()
    {
        // Act — tests the heap-alloc path (> 256)
        var code = CryptoHelper.GenerateRandomCode(512, "ABC");

        // Assert
        Assert.Equal(512, code.Length);
    }

    // ── ComputeSha256 ─────────────────────────────────────────────────────────

    [Fact]
    public void ComputeSha256_ReturnsCorrectHashLength()
    {
        // Act
        var hash = CryptoHelper.ComputeSha256("hello");

        // Assert — SHA-256 always produces 32 bytes
        Assert.Equal(32, hash.Length);
    }

    [Fact]
    public void ComputeSha256_SameInput_SameOutput()
    {
        // Act
        var hash1 = CryptoHelper.ComputeSha256("test-input");
        var hash2 = CryptoHelper.ComputeSha256("test-input");

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeSha256_DifferentInput_DifferentOutput()
    {
        // Act
        var hash1 = CryptoHelper.ComputeSha256("input-a");
        var hash2 = CryptoHelper.ComputeSha256("input-b");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeSha256_KnownVector_ProducesExpectedHash()
    {
        // Arrange — "hello" SHA-256 = 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
        var expected = Convert.FromHexString("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824");

        // Act
        var actual = CryptoHelper.ComputeSha256("hello");

        // Assert
        Assert.Equal(expected, actual);
    }
}
