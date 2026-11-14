using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.Files.Application;
using Microsoft.Extensions.Options;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 根据配置的环境名、年月和文件 ID 生成存储键
/// </summary>
internal sealed class StorageKeyFactory(IOptions<FilesStorageOptions> options) : IStorageKeyFactory
{
    public string KeyFor(Guid fileId, DateTimeOffset at) => FileStorageKeys.For(options.Value.Environment, fileId, at);
}
