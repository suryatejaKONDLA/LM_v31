using System.Net;
using System.Security.Cryptography;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CITL.Application.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CITL.Infrastructure.Core.FileStorage;

/// <summary>
/// Cloudflare R2 file storage provider — uses AWSSDK.S3 with a custom endpoint.
/// R2 is S3-compatible, so all standard S3 operations work.
/// </summary>
internal sealed partial class R2FileStorageProvider : IFileStorageProvider, IDisposable
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName;
    private readonly int _defaultExpiryMinutes;
    private readonly ILogger<R2FileStorageProvider> _logger;

    /// <summary>Initializes the R2 provider with configuration and logger.</summary>
    public R2FileStorageProvider(
        IOptions<FileStorageSettings> options,
        ILogger<R2FileStorageProvider> logger)
    {
        var settings = options.Value;
        _bucketName = settings.R2BucketName;
        _defaultExpiryMinutes = settings.SignedUrlExpiryMinutes;
        _logger = logger;

        var credentials = new BasicAWSCredentials(settings.R2AccessKey, settings.R2SecretKey);

        var config = new AmazonS3Config
        {
            ServiceURL = settings.R2Endpoint,
            ForcePathStyle = true,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };

        _s3Client = new(credentials, config);
    }

    /// <inheritdoc />
    public async Task<StoredFileMetadata> UploadAsync(
        string path,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        // Compute hash while buffering to a temp stream (S3 needs seekable or known length)
        string hash;
        long sizeInBytes;

        using var tempStream = new MemoryStream();
        using (var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
        {
            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await tempStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken)
                    .ConfigureAwait(false);

                sha256.AppendData(buffer, 0, bytesRead);
            }

            sizeInBytes = tempStream.Length;
            hash = Convert.ToHexString(sha256.GetCurrentHash());
        }

        tempStream.Position = 0;

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = NormalizeKey(path),
            InputStream = tempStream,
            ContentType = contentType
        };

        // Cloudflare R2 does not support STREAMING-AWS4-HMAC-SHA256-PAYLOAD (chunked encoding).
        // DisablePayloadSigning sends UNSIGNED-PAYLOAD in x-amz-content-sha256 header instead,
        // and ContentLength tells the SDK the full size so it avoids chunked transfer encoding.
        putRequest.DisablePayloadSigning = true;
        putRequest.Headers.ContentLength = sizeInBytes;

        putRequest.Metadata.Add("x-amz-meta-sha256", hash);

        await _s3Client.PutObjectAsync(putRequest, cancellationToken).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var fileName = Path.GetFileName(path);

        return new()
        {
            FileName = fileName,
            FilePath = path,
            Extension = Path.GetExtension(fileName),
            ContentType = contentType,
            SizeInBytes = sizeInBytes,
            Hash = hash,
            CreatedAtUtc = now,
            LastModifiedAtUtc = now
        };
    }

    /// <inheritdoc />
    public async Task<FileDownloadResult> DownloadAsync(string path, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = NormalizeKey(path)
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);

        return new()
        {
            Content = response.ResponseStream,
            ContentType = response.Headers.ContentType ?? "application/octet-stream",
            FileName = Path.GetFileName(path),
            SizeInBytes = response.ContentLength
        };
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = NormalizeKey(path)
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);

        LogFileDeleted(_logger, path);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = NormalizeKey(path)
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<StoredFileMetadata?> GetMetadataAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = NormalizeKey(path)
            };

            var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken)
                .ConfigureAwait(false);

            var fileName = Path.GetFileName(path);
            var hash = response.Metadata["x-amz-meta-sha256"] ?? string.Empty;

            return new()
            {
                FileName = fileName,
                FilePath = path,
                Extension = Path.GetExtension(fileName),
                ContentType = response.Headers.ContentType ?? "application/octet-stream",
                SizeInBytes = response.ContentLength,
                Hash = hash,
                CreatedAtUtc = response.LastModified?.ToUniversalTime() ?? DateTime.UtcNow,
                LastModifiedAtUtc = response.LastModified?.ToUniversalTime() ?? DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<FolderContents> ListAsync(string folderPath, CancellationToken cancellationToken)
    {
        var prefix = NormalizeKey(folderPath);

        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('/'))
        {
            prefix += '/';
        }

        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = prefix,
            Delimiter = "/"
        };

        var items = new List<FolderItem>();

        ListObjectsV2Response response;

        do
        {
            response = await _s3Client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

            // Sub-folders (common prefixes)
            foreach (var commonPrefix in response.CommonPrefixes ?? [])
            {
                var folderName = commonPrefix.TrimEnd('/');

                if (folderName.Contains('/'))
                {
                    folderName = folderName[(folderName.LastIndexOf('/') + 1)..];
                }

                items.Add(new()
                {
                    Name = folderName,
                    Path = commonPrefix.TrimEnd('/'),
                    IsFolder = true,
                    SizeInBytes = 0,
                    ContentType = string.Empty,
                    LastModifiedAtUtc = DateTime.MinValue
                });
            }

            // Files
            foreach (var s3Object in response.S3Objects ?? [])
            {
                // Skip the folder marker itself
                if (s3Object.Key == prefix)
                {
                    continue;
                }

                var fileName = Path.GetFileName(s3Object.Key);

                items.Add(new()
                {
                    Name = fileName,
                    Path = s3Object.Key,
                    IsFolder = false,
                    SizeInBytes = s3Object.Size ?? 0,
                    ContentType = "application/octet-stream", // S3 list doesn't return content type
                    LastModifiedAtUtc = s3Object.LastModified?.ToUniversalTime() ?? DateTime.UtcNow
                });
            }

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return new()
        {
            FolderPath = folderPath,
            Items = items,
            TotalCount = items.Count
        };
    }

    /// <inheritdoc />
    public async Task<SignedUrlResult> GetSignedUrlAsync(
        string path,
        int expiryMinutes,
        CancellationToken cancellationToken)
    {
        var expiry = expiryMinutes > 0 ? expiryMinutes : _defaultExpiryMinutes;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = NormalizeKey(path),
            Expires = DateTime.UtcNow.AddMinutes(expiry),
            Verb = HttpVerb.GET
        };

        var url = await _s3Client.GetPreSignedURLAsync(request).ConfigureAwait(false);
        var expiresAt = request.Expires ?? DateTime.UtcNow.AddMinutes(expiry);

        return new()
        {
            Url = url,
            ExpiresAtUtc = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FolderItem>> ListAllRecursiveAsync(
        string folderPath,
        CancellationToken cancellationToken)
    {
        var prefix = NormalizeKey(folderPath);

        if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('/'))
        {
            prefix += '/';
        }

        // No delimiter → returns ALL objects recursively under the prefix
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = prefix
        };

        var items = new List<FolderItem>();

        ListObjectsV2Response response;

        do
        {
            response = await _s3Client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

            foreach (var s3Object in response.S3Objects ?? [])
            {
                // Skip folder markers (zero-byte objects ending with /)
                if (s3Object.Key.EndsWith('/'))
                {
                    continue;
                }

                var fileName = Path.GetFileName(s3Object.Key);

                items.Add(new()
                {
                    Name = fileName,
                    Path = s3Object.Key,
                    IsFolder = false,
                    SizeInBytes = s3Object.Size ?? 0,
                    ContentType = "application/octet-stream",
                    LastModifiedAtUtc = s3Object.LastModified?.ToUniversalTime() ?? DateTime.UtcNow
                });
            }

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return items;
    }

    /// <inheritdoc />
    public async Task CreateFolderAsync(string folderPath, CancellationToken cancellationToken)
    {
        // S3/R2 doesn't have real folders — create a zero-byte marker object
        var key = NormalizeKey(folderPath);

        if (!key.EndsWith('/'))
        {
            key += '/';
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            ContentBody = string.Empty
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken).ConfigureAwait(false);

        LogFolderCreated(_logger, folderPath);
    }

    /// <inheritdoc />
    public void Dispose() => _s3Client.Dispose();

    // ─── Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Normalizes a path to an S3 key — forward slashes, no leading slash.
    /// </summary>
    private static string NormalizeKey(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    // ─── Source-generated log messages ──────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "R2 file deleted: {FilePath}")]
    private static partial void LogFileDeleted(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "R2 folder marker created: {FolderPath}")]
    private static partial void LogFolderCreated(ILogger logger, string folderPath);
}
