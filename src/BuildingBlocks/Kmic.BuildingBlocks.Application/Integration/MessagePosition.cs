namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 死信列表里的位置：发生时间加 ID，跨模块归并翻页时用
/// </summary>
/// <param name="OccurredAt">发生时间</param>
/// <param name="Id">行 ID</param>
public sealed record MessagePosition(DateTimeOffset OccurredAt, Guid Id);
