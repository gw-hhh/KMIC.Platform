using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Scheduling;

/// <summary>测试进程内定时任务触发辅助工具。</summary>
public static class SchedulerHarness
{
    /// <summary>绕过 Quartz 调度器，按指定计划时刻直接执行一次定时任务并记录执行结果。</summary>
    public static Task<Result<ScheduledJobOutcome>> FireAsync(this IServiceProvider services, string name, DateTimeOffset scheduledFireTime, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.GetRequiredService<ScheduledTasks>().Contains(name))
        {
            throw new InvalidOperationException($"未声明定时任务 {name}。");
        }

        return services.GetRequiredService<ScheduledCommandRunner>().RunAsync(
            name, new ScheduledRun(scheduledFireTime, ScheduledTrigger.Schedule), scheduledFireTime, null, "test", ct);
    }
}
