using CITL.SharedKernel.Helpers;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="PathSanitizer"/>.
/// Security-critical — validates traversal prevention, reserved name blocking,
/// control character rejection, and cross-platform path normalization.
/// </summary>
public sealed class PathSanitizerTests
{
    // ── SanitizePath — Valid inputs ────────────────────────────────────────────

    [Fact]
    public void SanitizePath_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = PathSanitizer.SanitizePath("");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizePath_Whitespace_ReturnsEmpty()
    {
        // Act
        var result = PathSanitizer.SanitizePath("   ");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizePath_SimplePath_ReturnsNormalized()
    {
        // Act
        var result = PathSanitizer.SanitizePath("invoices/2026");

        // Assert
        Assert.Equal("invoices/2026", result);
    }

    [Fact]
    public void SanitizePath_BackslashesNormalized_ToForwardSlash()
    {
        // Act
        var result = PathSanitizer.SanitizePath("invoices\\2026\\march");

        // Assert
        Assert.Equal("invoices/2026/march", result);
    }

    [Fact]
    public void SanitizePath_LeadingTrailingSlashes_Trimmed()
    {
        // Act
        var result = PathSanitizer.SanitizePath("/invoices/2026/");

        // Assert
        Assert.Equal("invoices/2026", result);
    }

    // ── SanitizePath — Traversal attacks ──────────────────────────────────────

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("invoices/../../etc/shadow")]
    [InlineData("..")]
    [InlineData("foo/../bar")]
    public void SanitizePath_TraversalAttempt_ReturnsNull(string path)
    {
        // Act
        var result = PathSanitizer.SanitizePath(path);

        // Assert
        Assert.Null(result);
    }

    // ── SanitizePath — Absolute paths ─────────────────────────────────────────

    [Theory]
    [InlineData("C:/Windows/System32")]
    [InlineData("D:\\data\\secrets")]
    public void SanitizePath_DriveLetterAbsolutePath_ReturnsNull(string path)
    {
        // Act
        var result = PathSanitizer.SanitizePath(path);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("//server/share")]
    [InlineData("\\\\server\\share")]
    public void SanitizePath_UncPath_ReturnsNull(string path)
    {
        // Act
        var result = PathSanitizer.SanitizePath(path);

        // Assert
        Assert.Null(result);
    }

    // ── SanitizePath — Control characters ─────────────────────────────────────

    [Theory]
    [InlineData("test\0file")]
    [InlineData("test\nfile")]
    [InlineData("test\rfile")]
    public void SanitizePath_ControlCharacters_ReturnsNull(string path)
    {
        // Act
        var result = PathSanitizer.SanitizePath(path);

        // Assert
        Assert.Null(result);
    }

    // ── SanitizePath — Reserved device names ──────────────────────────────────

    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT1")]
    [InlineData("con")]
    public void SanitizePath_ReservedDeviceName_ReturnsNull(string path)
    {
        // Act
        var result = PathSanitizer.SanitizePath(path);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizePath_ReservedNameInSegment_ReturnsNull()
    {
        // Act
        var result = PathSanitizer.SanitizePath("data/CON/files");

        // Assert
        Assert.Null(result);
    }

    // ── SanitizePath — Edge cases ─────────────────────────────────────────────

    [Fact]
    public void SanitizePath_DoubleSlashes_ReturnsNull()
    {
        // Act
        var result = PathSanitizer.SanitizePath("data//files");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizePath_DotOnlySegment_ReturnsNull()
    {
        // Act
        var result = PathSanitizer.SanitizePath("data/./files");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizePath_ExceedsMaxLength_ReturnsNull()
    {
        // Arrange — 1025 chars exceeds the 1024 limit
        var longPath = string.Join("/", Enumerable.Repeat("abcdefghij", 103));

        // Act
        var result = PathSanitizer.SanitizePath(longPath);

        // Assert
        Assert.Null(result);
    }

    // ── SanitizeFileName — Valid inputs ────────────────────────────────────────

    [Fact]
    public void SanitizeFileName_ValidName_ReturnsSame()
    {
        // Act
        var result = PathSanitizer.SanitizeFileName("report.pdf");

        // Assert
        Assert.Equal("report.pdf", result);
    }

    [Fact]
    public void SanitizeFileName_WithDirectoryComponents_StripsDirectory()
    {
        // Act
        var result = PathSanitizer.SanitizeFileName("path/to/report.pdf");

        // Assert
        Assert.Equal("report.pdf", result);
    }

    // ── SanitizeFileName — Invalid inputs ─────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeFileName_NullOrWhitespace_ReturnsNull(string? fileName)
    {
        // Act
        var result = PathSanitizer.SanitizeFileName(fileName!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizeFileName_ControlCharacters_ReturnsNull()
    {
        // Act
        var result = PathSanitizer.SanitizeFileName("file\0name.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizeFileName_DotOnly_ReturnsNull()
    {
        // Act
        var result = PathSanitizer.SanitizeFileName("...");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("CON.txt")]
    [InlineData("prn.pdf")]
    [InlineData("AUX.log")]
    [InlineData("NUL.dat")]
    [InlineData("COM1.doc")]
    [InlineData("LPT9.csv")]
    public void SanitizeFileName_ReservedDeviceName_ReturnsNull(string fileName)
    {
        // Act
        var result = PathSanitizer.SanitizeFileName(fileName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SanitizeFileName_ExceedsMaxLength_ReturnsNull()
    {
        // Arrange — 256 chars exceeds the 255 limit
        var longName = new string('a', 252) + ".txt";

        // Act
        var result = PathSanitizer.SanitizeFileName(longName);

        // Assert
        Assert.Null(result);
    }

    // ── IsReservedDeviceName ──────────────────────────────────────────────────

    [Theory]
    [InlineData("CON", true)]
    [InlineData("con", true)]
    [InlineData("Con", true)]
    [InlineData("PRN", true)]
    [InlineData("AUX", true)]
    [InlineData("NUL", true)]
    [InlineData("COM1", true)]
    [InlineData("COM9", true)]
    [InlineData("LPT1", true)]
    [InlineData("LPT9", true)]
    [InlineData("CONX", false)]
    [InlineData("COM10", false)]
    [InlineData("myfile", false)]
    [InlineData("document", false)]
    public void IsReservedDeviceName_ReturnsExpected(string name, bool expected)
    {
        // Act
        var result = PathSanitizer.IsReservedDeviceName(name);

        // Assert
        Assert.Equal(expected, result);
    }
}
