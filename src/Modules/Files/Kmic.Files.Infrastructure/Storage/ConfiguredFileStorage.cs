using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 按部署配置选择本地或 S3 存储，并转发存储操作
/// </summary>
/// <remarks>
/// 构造时选择存储并校验其配置；错误在创建服务时暴露
/// </remarks>
internal sealed class ConfiguredFileStorage : IFileStorage, IDisposable
{
    private readonly IFileStorage _inner;

    public ConfiguredFileStorage(IOptions<FilesStorageOptions> options, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(time);

        var settings = options.Value;
        if (string.Equals(settings.Provider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            _inner = new S3FileStorage(settings.S3, time);
            return;
        }

        var root = settings.LocalRoot is { Length: > 0 } configured ? configured : Path.Combine(Path.GetTempPath(), "kmic-files");
        _inner = new LocalFileStorage(root);
    }

    public Task<StoredObject> WriteAsync(string key, Stream content, string contentType, CancellationToken ct) =>
        _inner.WriteAsync(key, content, contentType, ct);

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct) => _inner.OpenReadAsync(key, ct);

    public Task DeleteAsync(string key, CancellationToken ct) => _inner.DeleteAsync(key, ct);

    public Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct) => _inner.ExistsAsync(key, versionId, ct);

    public Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct) =>
        _inner.GetDownloadUrlAsync(key, validFor, downloadName, ct);

    public IAsyncEnumerable<string> ListAsync(string prefix, CancellationToken ct) => _inner.ListAsync(prefix, ct);

    public void Dispose() => (_inner as IDisposable)?.Dispose();
}
