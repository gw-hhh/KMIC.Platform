namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>写入存储后的对象元数据</summary>
/// <param name="Key">存储键</param>
/// <param name="VersionId">对象版本号，本地存储为空</param>
public sealed record StoredObject(string Key, string? VersionId);
