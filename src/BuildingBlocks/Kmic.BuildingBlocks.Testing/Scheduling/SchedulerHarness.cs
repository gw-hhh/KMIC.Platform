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
    /// 不经过 Quartz，直接按计划时刻发送一次定时任务的命令，照常写执行记录
    /// </summary>
    /// <remarks>
    /// 业务模块的定时任务测试用它：触发一次断言结果，再按同一时刻触发一次，结果应当不变
    /// </remarks>
    /// <param name="services">宿主的服务</param>
    /// <param name="name">定时任务名</param>
    /// <param name="scheduledFireTime">计划触发时刻</param>
    /// <param name="ct">取消</param>
    public static Task<Result<ScheduledJobOutcome>> FireAsync(this IServiceProvider services, string name, DateTimeOffset scheduledFireTime, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var job = services.GetRequiredService<ScheduledJobCatalog>().Find(name)
            ?? throw new InvalidOperationException($"没有声明定时任务 {name}。");
        return services.GetRequiredService<ScheduledCommandRunner>().RunAsync(
            job, new ScheduledRun(scheduledFireTime, ScheduledTrigger.Schedule), scheduledFireTime, null, "test", ct);
    }
}
