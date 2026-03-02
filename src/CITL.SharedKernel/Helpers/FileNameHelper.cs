using System.Globalization;

namespace CITL.SharedKernel.Helpers;

/// <summary>
/// Filename resolution and placeholder replacement utilities for file storage.
/// All methods are pure functions with no side effects.
/// </summary>
public static class FileNameHelper
{
    /// <summary>
    /// Resolves the final filename using priority: custom name → auto-generate → original.
    /// Supports <c>{id}</c> and <c>{dt}</c> placeholders and auto-appends extension when missing.
    /// </summary>
    /// <param name="originalFileName">The original uploaded filename (used for extension fallback).</param>
    /// <param name="customFileName">Optional custom name with placeholder support.</param>
    /// <param name="autoGenerate">When <c>true</c> and no custom name is given, generates a GUID filename.</param>
    public static string ResolveFileName(string originalFileName, string? customFileName, bool autoGenerate)
    {
        var originalExtension = Path.GetExtension(originalFileName);

        // 1. Custom fileName provided → resolve placeholders, ensure extension
        if (!string.IsNullOrWhiteSpace(customFileName))
        {
            var resolved = ReplacePlaceholders(customFileName);

            if (string.IsNullOrEmpty(Path.GetExtension(resolved)))
            {
                resolved += originalExtension;
            }

            return resolved;
        }

        // 2. Auto-generate → GUID + original extension
        if (autoGenerate)
        {
            return $"{Guid.NewGuid():N}{originalExtension}";
        }

        // 3. Default → original filename
        return originalFileName;
    }

    /// <summary>
    /// Replaces supported placeholders in a filename:
    /// <c>{id}</c> → 32-char GUID (no hyphens), <c>{dt}</c> → UTC timestamp (yyyyMMddHHmmss).
    /// Matching is case-insensitive.
    /// </summary>
    public static string ReplacePlaceholders(string fileName)
    {
        var result = fileName;

        if (result.Contains("{id}", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Replace("{id}", Guid.NewGuid().ToString("N"), StringComparison.OrdinalIgnoreCase);
        }

        if (result.Contains("{dt}", StringComparison.OrdinalIgnoreCase))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            result = result.Replace("{dt}", timestamp, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
