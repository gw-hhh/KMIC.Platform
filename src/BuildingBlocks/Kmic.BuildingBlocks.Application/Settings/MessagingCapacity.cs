namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>数据库已分配存储容量（字节）及最后更新时间。</summary>
public sealed record MessagingCapacity(long Bytes, DateTimeOffset UpdatedAt);
