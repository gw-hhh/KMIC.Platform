using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Infrastructure.Files;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 从文件存储读取下载响应的内容
/// </summary>
internal sealed class FileContentReader(IFileStorage storage) : IFileContentReader
{
    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        return storage.OpenReadAsync(storageKey, ct);
    }
}
