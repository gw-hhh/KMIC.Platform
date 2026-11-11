namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 写入存储后的对象
/// </summary>
/// <param name="Key">存储键</param>
/// <param name="VersionId">对象版本号，桶开启版本控制时才有，本地存储为空</param>
public sealed record StoredObject(string Key, string? VersionId);
