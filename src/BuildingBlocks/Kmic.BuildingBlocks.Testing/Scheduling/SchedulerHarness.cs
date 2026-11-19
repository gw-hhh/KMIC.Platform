using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Scheduling;

/// <summary>
/// 在测试进程里触发定时任务
/// </summary>
public static class SchedulerHarness
{
    /// <summary>
    /// 不经过 Quartz，直接按计划时刻执行一次定时任务（模块的命令或框架任务），照常写执行记录
    /// </summary>
    public static Task<Result<ScheduledJobOutcome>> FireAsync(this IServiceProvider services, string name, DateTimeOffset scheduledFireTime, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.GetRequiredService<ScheduledTasks>().Contains(name))
        {
            throw new InvalidOperationException($"没有声明定时任务 {name}。");
        }

        return services.GetRequiredService<ScheduledCommandRunner>().RunAsync(
            name, new ScheduledRun(scheduledFireTime, ScheduledTrigger.Schedule), scheduledFireTime, null, "test", ct);
    }
}
