using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.Purge;

/// <summary>
/// 清理过期的站内消息：已读的超过保留期、一直没读的超过更长的保留期
/// </summary>
/// <remarks>
/// 每天执行一次，保留期见 Notifications:Retention
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeSiteMessagesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
