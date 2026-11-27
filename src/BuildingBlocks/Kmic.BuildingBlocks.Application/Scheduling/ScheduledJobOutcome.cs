namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>定时任务命令执行结果，将写入执行记录。</summary>
public sealed record ScheduledJobOutcome(int ItemsProcessed, string? Note = null);
