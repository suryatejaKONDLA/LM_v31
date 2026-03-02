using CITL.SharedKernel.Helpers;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="ImageHelper"/>.
/// Validates MIME type detection from magic bytes (file signatures).
/// </summary>
public sealed class ImageHelperTests
{
    // ── JPEG ──────────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_JpegSignature_ReturnsJpeg()
    {
        // Arrange — FF D8 FF E0 (JFIF)
        var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/jpeg", result);
    }

    [Fact]
    public void DetectMimeType_JpegExifSignature_ReturnsJpeg()
    {
        // Arrange — FF D8 FF E1 (EXIF)
        var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x00 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/jpeg", result);
    }

    // ── PNG ───────────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_PngSignature_ReturnsPng()
    {
        // Arrange — 89 50 4E 47
        var data = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/png", result);
    }

    // ── GIF ───────────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_GifSignature_ReturnsGif()
    {
        // Arrange — 47 49 46 (GIF)
        var data = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/gif", result);
    }

    // ── BMP ───────────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_BmpSignature_ReturnsBmp()
    {
        // Arrange — 42 4D (BM)
        var data = new byte[] { 0x42, 0x4D, 0x00, 0x00, 0x00, 0x00 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/bmp", result);
    }

    // ── WebP ──────────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_WebPSignature_ReturnsWebP()
    {
        // Arrange — RIFF....WEBP
        var data = new byte[]
        {
            0x52, 0x49, 0x46, 0x46, // RIFF
            0x00, 0x00, 0x00, 0x00, // file size (don't care)
            0x57, 0x45, 0x42, 0x50  // WEBP
        };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/webp", result);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void DetectMimeType_NullData_ReturnsPng()
    {
        // Act
        var result = ImageHelper.DetectMimeType(null);

        // Assert — default fallback
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void DetectMimeType_EmptyArray_ReturnsPng()
    {
        // Act
        var result = ImageHelper.DetectMimeType([]);

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void DetectMimeType_TooShort_ReturnsPng()
    {
        // Arrange — less than 4 bytes
        var data = new byte[] { 0xFF, 0xD8 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void DetectMimeType_UnknownFormat_ReturnsPng()
    {
        // Arrange — random bytes that don't match any signature
        var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };

        // Act
        var result = ImageHelper.DetectMimeType(data);

        // Assert — defaults to PNG
        Assert.Equal("image/png", result);
    }
}
