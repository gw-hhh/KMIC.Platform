using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 框架唯一的 Quartz 作业：按任务名找到声明，发送它的命令
/// </summary>
/// <remarks>
/// 同一个任务不并发执行。进程崩溃时不自动恢复，等下一次触发或手动执行：一次执行就是一个命令事务，没提交就没有副作用
/// </remarks>
[DisallowConcurrentExecution]
internal sealed class CommandJob(ScheduledJobCatalog catalog, ScheduledCommandRunner runner, ILogger<CommandJob> logger) : IJob
{
    /// <summary>
    /// 框架任务在 Quartz 里的分组
    /// </summary>
    public const string Group = "Kmic";

    /// <summary>
    /// 手动执行时放在触发数据里，值是操作人，系统发起时为空字符串
    /// </summary>
    public const string TriggeredByKey = "kmic.triggeredBy";

    /// <summary>
    /// 实际触发晚于计划这么久就算补执行，与调度器的错过阈值一致
    /// </summary>
    public static readonly TimeSpan MisfireThreshold = TimeSpan.FromMinutes(1);

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var name = context.JobDetail.Key.Name;
        if (catalog.Find(name) is not { } job)
        {
            // 滚动发布时新旧实例声明的任务不一样，交给认识它的实例执行
            SchedulingLog.UnknownJob(logger, name);
            return;
        }

        var scheduled = context.ScheduledFireTimeUtc ?? context.FireTimeUtc;
        UserId? triggeredBy = null;
        ScheduledTrigger trigger;
        if (context.Trigger.JobDataMap.TryGetValue(TriggeredByKey, out var by))
        {
            trigger = ScheduledTrigger.Manual;
            triggeredBy = Guid.TryParse(by as string, out var user) ? UserId.From(user) : null;
        }
        else
        {
            trigger = context.FireTimeUtc - scheduled > MisfireThreshold ? ScheduledTrigger.Misfire : ScheduledTrigger.Schedule;
        }

        await runner.RunAsync(job, new ScheduledRun(scheduled, trigger), context.FireTimeUtc, triggeredBy, context.Scheduler.SchedulerInstanceId, cancellationToken);
    }
}
