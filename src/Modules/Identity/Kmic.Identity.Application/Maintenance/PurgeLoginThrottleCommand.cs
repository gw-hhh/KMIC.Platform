using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Maintenance;

/// <summary>清理超过重置间隔且不再受等待或锁定限制的登录失败计数</summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeLoginThrottleCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
