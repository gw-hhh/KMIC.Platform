using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Maintenance;

/// <summary>
/// 清理登录失败计数：最近一次失败已经过了重置时间，而且没有处在等待或锁定中的行
/// </summary>
/// <remarks>
/// 每天执行。不清理的话，每个输错过密码的登录名（包括不存在的）都会在表里留一行
/// </remarks>
/// <param name="ScheduledFireTime">计划的触发时间</param>
[SystemOnly]
[NotAudited("定时清理，结果记入作业运行记录")]
public sealed record PurgeLoginThrottleCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
