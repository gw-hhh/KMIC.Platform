using System.Net;
using System.Runtime.CompilerServices;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// S3 兼容的对象存储，生产是阿里云 OSS，开发和测试是 MinIO
/// </summary>
/// <remarks>
/// 完整性校验只在 SDK 认为必需时才做：兼容存储不支持新版 SDK 默认附带的校验头。
/// 写入按分段上传，不把整个文件读进内存；桶开启版本控制时，写入返回对象的版本号
/// </remarks>
public sealed class S3FileStorage : IFileStorage, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;
    private readonly int _partSize;
    private readonly TimeProvider _time;
    private readonly bool _plainHttp;

    /// <summary>
    /// 按配置创建
    /// </summary>
    public S3FileStorage(S3FileStorageOptions options, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(time);

        _bucket = options.Bucket;
        _partSize = options.PartSizeBytes;
        _time = time;
        _plainHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
        _client = new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region,
                ForcePathStyle = options.ForcePathStyle,
                UseHttp = _plainHttp,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            });
    }

    /// <inheritdoc />
    public async Task<StoredObject> WriteAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(content);

        var first = new byte[_partSize];
        var firstLength = await ReadFullAsync(content, first, ct);
        if (firstLength < _partSize)
        {
            using var whole = new MemoryStream(first, 0, firstLength, writable: false);
            var put = await _client.PutObjectAsync(new PutObjectRequest { BucketName = _bucket, Key = key, InputStream = whole, ContentType = contentType }, ct);
            return new StoredObject(key, NullIfEmpty(put.VersionId));
        }

        return await WriteMultipartAsync(key, content, contentType, first, ct);
    }

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetObjectAsync(_bucket, key, ct);
            return new ResponseStream(response);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"存储里没有 {key}。", key, exception);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken ct) =>
        await _client.DeleteObjectAsync(_bucket, key, ct);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct)
    {
        try
        {
            await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = _bucket, Key = key, VersionId = versionId }, ct);
            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadName);

        var url = await _client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Protocol = _plainHttp ? Protocol.HTTP : Protocol.HTTPS,
            Expires = _time.GetUtcNow().UtcDateTime + validFor,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(downloadName)}",
            },
        });
        return new Uri(url);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ListAsync(string prefix, [EnumeratorCancellation] CancellationToken ct)
    {
        string? token = null;
        do
        {
            var page = await _client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = _bucket, Prefix = prefix, ContinuationToken = token }, ct);
            foreach (var item in page.S3Objects ?? [])
            {
                yield return item.Key;
            }

            token = page.IsTruncated == true ? page.NextContinuationToken : null;
        }
        while (token is not null);
    }

    /// <inheritdoc />
    public void Dispose() => _client.Dispose();

    // 读满缓冲区或读到流结束，返回读到的字节数
    private static async Task<int> ReadFullAsync(Stream stream, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total), ct);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private async Task<StoredObject> WriteMultipartAsync(string key, Stream content, string contentType, byte[] first, CancellationToken ct)
    {
        var initiated = await _client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest { BucketName = _bucket, Key = key, ContentType = contentType }, ct);
        try
        {
            var parts = new List<PartETag>();
            var buffer = first;
            var length = first.Length;
            for (var number = 1; length > 0; number++)
            {
                using var part = new MemoryStream(buffer, 0, length, writable: false);
                var uploaded = await _client.UploadPartAsync(
                    new UploadPartRequest { BucketName = _bucket, Key = key, UploadId = initiated.UploadId, PartNumber = number, InputStream = part, PartSize = length },
                    ct);
                parts.Add(new PartETag(number, uploaded.ETag));

                buffer = new byte[_partSize];
                length = await ReadFullAsync(content, buffer, ct);
            }

            var completed = await _client.CompleteMultipartUploadAsync(
                new CompleteMultipartUploadRequest { BucketName = _bucket, Key = key, UploadId = initiated.UploadId, PartETags = parts },
                ct);
            return new StoredObject(key, NullIfEmpty(completed.VersionId));
        }
        catch
        {
            // 出错或取消时中止，免得未完成的分段一直占着空间
            await _client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest { BucketName = _bucket, Key = key, UploadId = initiated.UploadId }, CancellationToken.None);
            throw;
        }
    }

    // 读到哪里、释放时一并释放响应，连接才会归还
    private sealed class ResponseStream(GetObjectResponse response) : Stream
    {
        private readonly Stream _inner = response.ResponseStream;

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => response.ContentLength;

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _inner.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
