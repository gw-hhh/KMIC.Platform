using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;

namespace Kmic.BuildingBlocks.Hosting.Health;

/// <summary>
/// 定时任务调度器，只在执行定时任务的实例上检查
/// </summary>
internal sealed class SchedulerDependencyCheck(ISchedulerFactory schedulers) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var scheduler = await schedulers.GetScheduler(cancellationToken);
        return scheduler.Status switch
        {
            SchedulerStatus.Running => HealthCheckResult.Healthy("调度器运行中"),
            SchedulerStatus.Created or SchedulerStatus.Standby => HealthCheckResult.Degraded("调度器还没有开始触发任务"),
            _ => HealthCheckResult.Unhealthy("调度器已停止"),
        };
    }
}
