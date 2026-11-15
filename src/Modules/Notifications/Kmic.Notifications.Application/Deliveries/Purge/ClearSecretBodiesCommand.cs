using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Deliveries.Purge;

/// <summary>
/// 清空含密投递残留的正文和短信参数
/// </summary>
/// <remarks>
/// 清空已发送、失败或已跳过投递的残留内容；捕获内容由独立任务按保留期清理
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record ClearSecretBodiesCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
