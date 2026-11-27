using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

[DisallowConcurrentExecution]
internal sealed class CommandJob(ScheduledTasks tasks, ScheduledCommandRunner runner, ILogger<CommandJob> logger) : IJob
{
    public const string Group = "Kmic";

    // 手动触发时记录触发人，系统自动调度时为空字符串
    public const string TriggeredByKey = "kmic.triggeredBy";

    // 实际触发延迟超过此阈值判定为补执行（与调度器 misfire 阈值一致）
    public static readonly TimeSpan MisfireThreshold = TimeSpan.FromMinutes(1);

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var name = context.JobDetail.Key.Name;
        if (!tasks.Contains(name))
        {
            // 滚动发布期间新旧版本实例声明的任务可能存在差异，若当前实例未识别则由其他兼容实例接手执行
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

        await runner.RunAsync(name, new ScheduledRun(scheduled, trigger), context.FireTimeUtc, triggeredBy, context.Scheduler.SchedulerInstanceId, cancellationToken);
    }
}
