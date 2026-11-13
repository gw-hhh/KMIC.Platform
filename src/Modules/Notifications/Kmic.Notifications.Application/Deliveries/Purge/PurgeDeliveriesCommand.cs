using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 清理超过保留期的投递记录和发送请求记录
/// </summary>
/// <remarks>
/// 每天执行一次，保留期见 Notifications:Retention。只删已完成的投递：已发送、已跳过、已捕获和已放弃的失败；
/// 还在等人处理的失败投递不删。发送请求记录在它的投递都删完之后才删
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeDeliveriesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
