namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 数据库的已分配容量
/// </summary>
/// <param name="Bytes">已分配的存储容量（字节）</param>
/// <param name="UpdatedAt">最后一次更新的时间</param>
public sealed record MessagingCapacity(long Bytes, DateTimeOffset UpdatedAt);
