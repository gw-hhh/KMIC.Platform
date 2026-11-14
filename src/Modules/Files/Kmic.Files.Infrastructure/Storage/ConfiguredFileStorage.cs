using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 按部署配置在本地目录和 S3 之间选一种存储，其余全部转给选中的那个
/// </summary>
/// <remarks>
/// 选择在构造时做一次：配置不对（S3 缺桶名等）在第一次用到之前就暴露，而不是等到第一次上传
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
