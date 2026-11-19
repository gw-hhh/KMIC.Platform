using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.Purge;

/// <summary>
/// 按已读和未读各自的保留期清理站内消息，保留期见 Notifications:Retention
/// </summary>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeSiteMessagesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
