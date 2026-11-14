namespace Kmic.Files.Application;

/// <summary>
/// 存储里的一个对象
/// </summary>
/// <param name="Key">存储键</param>
/// <param name="FileId">键里的文件 ID</param>
/// <param name="CreatedAt">文件 ID 里的创建时间，ID 不带时间时为空</param>
public sealed record InventoryObject(string Key, Guid FileId, DateTimeOffset? CreatedAt);
