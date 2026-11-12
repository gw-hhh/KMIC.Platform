using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Maintenance;

/// <summary>
/// 清理已经结束的会话：超过绝对上限的，或作废超过保留期的，连同它们的刷新令牌
/// </summary>
/// <remarks>
/// 每天凌晨执行。会话表只保留最近发生的事，更久远的登录与退出在安全事件里
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeSessionsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
