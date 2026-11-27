using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Infrastructure.Files;

namespace Kmic.Files.Infrastructure.Storage;

internal sealed class FileContentReader(IFileStorage storage) : IFileContentReader
{
    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        return storage.OpenReadAsync(storageKey, ct);
    }
}
