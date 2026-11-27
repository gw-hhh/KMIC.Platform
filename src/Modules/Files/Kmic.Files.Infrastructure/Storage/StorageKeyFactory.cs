using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.Files.Application;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

internal sealed class StorageKeyFactory(IOptions<FilesStorageOptions> options) : IStorageKeyFactory
{
    public string KeyFor(Guid fileId, DateTimeOffset at) => FileStorageKeys.For(options.Value.Environment, fileId, at);
}
