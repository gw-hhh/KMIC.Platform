using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Captured.Purge;

/// <summary>
/// 清理超过保留期的捕获内容
/// </summary>
/// <remarks>
/// 每天按 Notifications:Retention 中的保留期清理捕获内容
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeCapturedCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
