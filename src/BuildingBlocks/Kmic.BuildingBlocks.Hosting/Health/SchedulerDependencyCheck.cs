using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;

namespace Kmic.BuildingBlocks.Hosting.Health;

// 仅在启用 Worker 角色的实例中注册执行
internal sealed class SchedulerDependencyCheck(ISchedulerFactory schedulers) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var scheduler = await schedulers.GetScheduler(cancellationToken);
        return scheduler.Status switch
        {
            SchedulerStatus.Running => HealthCheckResult.Healthy("调度器运行中"),
            SchedulerStatus.Created or SchedulerStatus.Standby => HealthCheckResult.Degraded("调度器尚未开始调度"),
            _ => HealthCheckResult.Unhealthy("调度器已停止"),
        };
    }
}
