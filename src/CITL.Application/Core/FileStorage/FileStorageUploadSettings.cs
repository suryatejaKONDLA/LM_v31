using System.Collections.Frozen;

namespace CITL.Application.Core.FileStorage;

/// <summary>
/// Upload-specific settings for file storage — allowed extensions, size limits, etc.
/// Populated from the "FileStorage" configuration section.
/// </summary>
public sealed class FileStorageUploadSettings
{
    /// <summary>
    /// Set of allowed file extensions (lowercase, including the leading dot).
    /// When empty, all extensions are permitted.
    /// </summary>
    public FrozenSet<string> AllowedExtensions { get; init; } = FrozenSet<string>.Empty;
}
