namespace CITL.Infrastructure.Core.FileStorage;

/// <summary>
/// Configuration for the file storage service — binds to the "FileStorage" section.
/// </summary>
public sealed class FileStorageSettings
{
    /// <summary>Configuration section name in appsettings.json.</summary>
    public const string SectionName = "FileStorage";

    /// <summary>Active provider: "Local" or "R2".</summary>
    public required string Provider { get; init; }

    /// <summary>Base path for local file storage (e.g. "./storage" or a shared drive UNC path).</summary>
    public string LocalBasePath { get; init; } = "./storage";

    /// <summary>Storage quota in GB for the local base path folder. Health check reports usage against this quota.</summary>
    public double LocalQuotaGB { get; init; } = 20.0;

    /// <summary>Cloudflare R2 S3-compatible endpoint URL.</summary>
    public string R2Endpoint { get; init; } = string.Empty;

    /// <summary>Cloudflare R2 access key ID.</summary>
    public string R2AccessKey { get; init; } = string.Empty;

    /// <summary>Cloudflare R2 secret access key.</summary>
    public string R2SecretKey { get; init; } = string.Empty;

    /// <summary>Cloudflare R2 bucket name.</summary>
    public string R2BucketName { get; init; } = string.Empty;

    /// <summary>Default signed URL expiry in minutes.</summary>
    public int SignedUrlExpiryMinutes { get; init; } = 15;

    /// <summary>Cloudflare R2 public domain for signed URLs (e.g. "https://files.example.com").</summary>
    public string R2PublicDomain { get; init; } = string.Empty;

    /// <summary>
    /// Allowed file extensions (without leading dot, case-insensitive).
    /// Example: <c>["jpg", "png", "pdf"]</c>. Empty array means all extensions are permitted.
    /// </summary>
    public string[] AllowedExtensions { get; init; } = [];
}
