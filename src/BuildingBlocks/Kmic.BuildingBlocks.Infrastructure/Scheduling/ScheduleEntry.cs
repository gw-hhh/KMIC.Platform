using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 同步到 Quartz 的一个任务：模块的定时任务和框架任务都是这个样子
/// </summary>
internal sealed record ScheduleEntry(string Name, string Cron, Misfire Misfire, string DisplayName);
