namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 本地磁盘存储，开发和测试用
/// </summary>
/// <remarks>
/// 根目录来自配置，不能放在站点目录里。存储键只允许由字母、数字、短横线、下划线和斜线组成的相对路径，
/// 防止写到根目录之外。先写临时文件再改名，读的人不会看到写了一半的文件
/// </remarks>
/// <param name="root">根目录</param>
public sealed class LocalFileStorage(string root) : IFileStorage
{
    private const string TempSuffix = ".writing";

    private readonly string _root = Path.GetFullPath(root);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = PathOf(key);
        return File.Exists(path)
            ? Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
            : throw new FileNotFoundException($"存储里没有 {key}。", path);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string key, CancellationToken ct)
    {
        File.Delete(PathOf(key));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct) =>
        Task.FromResult(File.Exists(PathOf(key)));

    /// <inheritdoc />
    public Task<Uri> GetDownloadUrlAsync(string key, TimeSpan validFor, string downloadName, CancellationToken ct) =>
        throw new NotSupportedException("本地存储不能生成下载地址，由应用转发文件。");

    /// <inheritdoc />
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
            throw new ArgumentException($"存储键不合法：“{key}”。", nameof(key));
        }

        var path = Path.GetFullPath(Path.Combine(_root, key));
        return path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? path
            : throw new ArgumentException($"存储键指到了根目录之外：“{key}”。", nameof(key));
    }
}
