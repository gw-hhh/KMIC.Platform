namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>作业列表游标位置，由提交时间与 ID 组成，用于跨模块归并分页。</summary>
public sealed record JobPosition(DateTimeOffset SubmittedAt, Guid Id);
