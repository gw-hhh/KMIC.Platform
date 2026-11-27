namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>本地磁盘文件存储，仅用于开发与测试环境（根目录不得位于站点发布目录内）</summary>
public sealed class LocalFileStorage(string root) : IFileStorage
{
    private const string TempSuffix = ".writing";

    private readonly string _root = Path.GetFullPath(root);

    public async Task<StoredObject> WriteAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path = PathOf(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.CreateVersion7().ToString("N") + TempSuffix;
        try
        {
            await using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(file, ct);
            }

            File.Move(temp, path, overwrite: false);
        }
        catch
        {
            File.Delete(temp);
            throw;
        }

        return new StoredObject(key, null);
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = PathOf(key);
        return File.Exists(path)
            ? Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
            : throw new FileNotFoundException($"存储中不存在对象 {key}。", path);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        File.Delete(PathOf(key));
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct) =>
        Task.FromResult(File.Exists(PathOf(key)));

    public Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct) =>
        throw new NotSupportedException("本地存储不支持生成直链下载地址，请通过应用接口转发文件。");

    public async IAsyncEnumerable<string> ListAsync(string prefix, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        if (!Directory.Exists(_root))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            if (file.EndsWith(TempSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            var key = Path.GetRelativePath(_root, file).Replace('\\', '/');
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                yield return key;
            }
        }
    }

    private string PathOf(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (key.StartsWith('/') || key.Contains("..", StringComparison.Ordinal) || key.Contains('\\', StringComparison.Ordinal) || key.Contains(':', StringComparison.Ordinal))
        {
            throw new ArgumentException($"无效的存储键：“{key}”。", nameof(key));
        }

        var path = Path.GetFullPath(Path.Combine(_root, key));
        return path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? path
            : throw new ArgumentException($"存储键越界访问根目录之外的路径：“{key}”。", nameof(key));
    }
}
