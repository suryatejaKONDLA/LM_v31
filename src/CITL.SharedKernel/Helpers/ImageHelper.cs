namespace CITL.SharedKernel.Helpers;

/// <summary>
/// Cross-cutting helper methods for image processing.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Detects the MIME type of an image from its magic bytes (file signature).
    /// Returns <c>image/png</c> as the default when the format cannot be determined.
    /// </summary>
    public static string DetectMimeType(byte[]? imageData)
    {
        if (imageData is null || imageData.Length < 4)
        {
            return "image/png";
        }

        // JPEG: FF D8 FF
        if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
        {
            return "image/jpeg";
        }

        // PNG: 89 50 4E 47
        if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
        {
            return "image/png";
        }

        // GIF: 47 49 46
        if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
        {
            return "image/gif";
        }

        // BMP: 42 4D
        if (imageData[0] == 0x42 && imageData[1] == 0x4D)
        {
            return "image/bmp";
        }

        // WebP: 52 49 46 46 ... 57 45 42 50
        if (imageData.Length >= 12
            && imageData[0] == 0x52 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x46
            && imageData[8] == 0x57 && imageData[9] == 0x45 && imageData[10] == 0x42 && imageData[11] == 0x50)
        {
            return "image/webp";
        }

        return "image/png";
    }
}
