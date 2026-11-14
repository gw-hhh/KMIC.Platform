using System.Runtime.CompilerServices;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.Files.Application;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// <see cref="IStorageInventory"/> 的实现：用文件存储，只看本环境的前缀
/// </summary>
internal sealed class StorageInventory(IFileStorage storage, IOptions<FilesStorageOptions> options) : IStorageInventory
{
    public async IAsyncEnumerable<InventoryObject> ListAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var environment = options.Value.Environment;
        await foreach (var key in storage.ListAsync(environment + "/", ct))
        {
            if (FileStorageKeys.TryGetFileId(key, environment, out var fileId))
            {
                yield return new InventoryObject(key, fileId, FileStorageKeys.CreatedAt(fileId));
            }
        }
    }

    public Task<bool> ExistsAsync(string key, string? versionId, CancellationToken ct) => storage.ExistsAsync(key, versionId, ct);

    public Task DeleteAsync(string key, CancellationToken ct) => storage.DeleteAsync(key, ct);
}
