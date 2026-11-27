using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>清空敏感投递残留的正文与短信参数</summary>
/// <param name="ScheduledFireTime">计划触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record ClearSecretBodiesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
