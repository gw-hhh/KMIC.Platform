using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// Worker 角色里运行调度器
/// </summary>
internal sealed class SchedulerService(ISchedulerFactory schedulers, StartupState startup) : BackgroundService
{
    private readonly Lock _gate = new();
    private Task? _shutdown;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        // 两次停机等同一个关闭任务，避免 Quartz 关到一半时容器已被另一次停机释放（测试宿主释放时会同时停两次）
        Task shutdown;
        lock (_gate)
        {
            shutdown = _shutdown ??= ShutdownAsync(cancellationToken);
        }

        await shutdown.WaitAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startup.WaitAsync(stoppingToken);
        var scheduler = await schedulers.GetScheduler(stoppingToken);
        await scheduler.Start(stoppingToken);
    }

    private async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        var scheduler = await schedulers.GetScheduler(cancellationToken);
        if (scheduler.Status is SchedulerStatus.Running or SchedulerStatus.Standby)
        {
            await scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);
        }
    }
}
