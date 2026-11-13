using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Infrastructure.Files;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 下载响应读取内容的入口：只转给文件存储
/// </summary>
internal sealed class FileContentReader(IFileStorage storage) : IFileContentReader
{
    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        return storage.OpenReadAsync(storageKey, ct);
    }
}
