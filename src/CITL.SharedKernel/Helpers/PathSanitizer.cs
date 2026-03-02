namespace CITL.SharedKernel.Helpers;

/// <summary>
/// Security-focused path and filename sanitization for cross-platform file storage.
/// Rejects directory traversal, reserved names, control characters, and overly long paths.
/// </summary>
public static class PathSanitizer
{
    private const int MaxPathLength = 1024;
    private const int MaxFileNameLength = 255;

    private static readonly string[] ReservedDeviceNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    /// <summary>
    /// Sanitizes a relative path — rejects traversal attacks (<c>..</c>), absolute paths,
    /// null bytes, control characters, and reserved names.
    /// Normalizes separators to forward-slash for cross-platform compatibility.
    /// Returns <c>null</c> if the path is invalid.
    /// </summary>
    public static string? SanitizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // Reject null bytes and control characters
        foreach (var c in path)
        {
            if (char.IsControl(c))
            {
                return null;
            }
        }

        // Normalize all separators to forward-slash (cross-platform)
        var normalized = path.Replace('\\', '/');

        // Reject UNC paths (//server/share or \\server\share) BEFORE trimming
        if (normalized.StartsWith("//", StringComparison.Ordinal))
        {
            return null;
        }

        normalized = normalized.Trim('/');

        if (string.IsNullOrEmpty(normalized))
        {
            return string.Empty;
        }

        // Reject directory traversal
        if (normalized.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        // Reject absolute paths: drive letters (C:) or Unix root (/)
        if (normalized is [_, ':', ..])
        {
            return null;
        }

        if (normalized.Length > MaxPathLength)
        {
            return null;
        }

        // Validate each segment
        var segments = normalized.Split('/');

        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                return null; // Double slashes or empty segments
            }

            // Reject segments that are only dots (. or ..)
            if (segment.AsSpan().TrimStart('.').IsEmpty)
            {
                return null;
            }

            if (IsReservedDeviceName(segment))
            {
                return null;
            }
        }

        return normalized;
    }

    /// <summary>
    /// Sanitizes a filename — strips directory components, rejects invalid characters,
    /// control characters, reserved names, and excessively long names.
    /// Returns <c>null</c> if the filename is invalid.
    /// </summary>
    public static string? SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        // Strip any directory components
        var name = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        // Reject control characters
        foreach (var c in name)
        {
            if (char.IsControl(c))
            {
                return null;
            }
        }

        // Reject names with invalid file name chars (platform-dependent set)
        var invalidChars = Path.GetInvalidFileNameChars();

        if (name.AsSpan().IndexOfAny(invalidChars) >= 0)
        {
            return null;
        }

        // Reject names that are only dots (. or ..)
        if (name.AsSpan().TrimStart('.').IsEmpty)
        {
            return null;
        }

        // Reject reserved device names (cross-platform safety)
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);

        if (IsReservedDeviceName(nameWithoutExtension))
        {
            return null;
        }

        if (name.Length > MaxFileNameLength)
        {
            return null;
        }

        return name;
    }

    /// <summary>
    /// Checks if a name matches a Windows reserved device name.
    /// Blocked on all platforms for cross-platform portability.
    /// </summary>
    public static bool IsReservedDeviceName(string name)
    {
        foreach (var r in ReservedDeviceNames)
        {
            if (name.Equals(r, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
