namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务命令的结果，写进执行记录
/// </summary>
/// <param name="ItemsProcessed">处理了多少条</param>
/// <param name="Note">给运维看的简短说明</param>
public sealed record ScheduledJobOutcome(int ItemsProcessed, string? Note = null);
