namespace Kmic.Files.Application;

/// <summary>存储对象</summary>
/// <param name="Key">存储键</param>
/// <param name="FileId">存储键中的文件 ID</param>
/// <param name="CreatedAt">文件 ID 中的创建时间，不包含时间时为空</param>
public sealed record InventoryObject(string Key, Guid FileId, DateTimeOffset? CreatedAt);
