namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>死信列表游标位置，由发生时间与行 ID 组成，用于跨模块归并分页。</summary>
public sealed record MessagePosition(DateTimeOffset OccurredAt, Guid Id);
