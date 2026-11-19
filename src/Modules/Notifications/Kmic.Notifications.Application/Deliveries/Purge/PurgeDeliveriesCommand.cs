using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 清理超过保留期的投递记录和发送请求记录
/// </summary>
/// <remarks>
/// 没放弃的失败投递不清
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeDeliveriesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
