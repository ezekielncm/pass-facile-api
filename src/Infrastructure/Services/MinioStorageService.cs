using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Services;

/// <summary>
/// Implémentation de <see cref="IStorageService"/> utilisant MinIO (S3-compatible).
/// </summary>
public sealed class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(
        IMinioClient client,
        IOptions<MinioSettings> settings,
        ILogger<MinioStorageService> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string objectName,
        string contentType,
        string? bucket = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = bucket ?? _settings.DefaultBucket;
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(args, cancellationToken);

        var publicUrl = $"{_settings.PublicUrl.TrimEnd('/')}/{bucketName}/{objectName}";
        _logger.LogInformation("Objet uploadé dans MinIO : {Bucket}/{ObjectName}", bucketName, objectName);

        return publicUrl;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string objectName,
        string? bucket = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = bucket ?? _settings.DefaultBucket;

        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        await _client.RemoveObjectAsync(args, cancellationToken);

        _logger.LogInformation("Objet supprimé de MinIO : {Bucket}/{ObjectName}", bucketName, objectName);
    }

    /// <inheritdoc />
    public async Task<string> GetPresignedUrlAsync(
        string objectName,
        int expiryInSeconds = 3600,
        string? bucket = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = bucket ?? _settings.DefaultBucket;

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);

        var url = await _client.PresignedGetObjectAsync(args);

        _logger.LogDebug("URL pré-signée générée pour {Bucket}/{ObjectName} (expiry={Expiry}s)",
            bucketName, objectName, expiryInSeconds);

        return url;
    }

    /// <summary>
    /// Crée le bucket s'il n'existe pas encore.
    /// </summary>
    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await _client.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _client.MakeBucketAsync(makeArgs, cancellationToken);
            _logger.LogInformation("Bucket MinIO créé : {Bucket}", bucketName);
        }
    }
}
