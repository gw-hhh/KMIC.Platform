using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Cleanup;

/// <summary>
/// 清理到期或撤销后超过保留期的会话及刷新令牌
/// </summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeSessionsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
