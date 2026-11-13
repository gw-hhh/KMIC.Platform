namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 作业列表里的位置：提交时间加 ID，跨模块归并翻页时用
/// </summary>
/// <param name="SubmittedAt">提交时间</param>
/// <param name="Id">作业 ID</param>
public sealed record JobPosition(DateTimeOffset SubmittedAt, Guid Id);
