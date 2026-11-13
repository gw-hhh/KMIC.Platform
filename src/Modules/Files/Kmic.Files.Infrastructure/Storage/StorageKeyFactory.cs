using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.Files.Application;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// <see cref="IStorageKeyFactory" /> 的实现：环境名加年月加文件 ID，环境名来自配置
/// </summary>
internal sealed class StorageKeyFactory(IOptions<FilesStorageOptions> options) : IStorageKeyFactory
{
    public string KeyFor(Guid fileId, DateTimeOffset at) => FileStorageKeys.For(options.Value.Environment, fileId, at);
}
