using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

internal sealed class SchedulerService(ISchedulerFactory schedulers, StartupState startup) : BackgroundService
{
    private readonly Lock _gate = new();
    private Task? _shutdown;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        // 多次调用停机时复用同一关闭任务，防止测试宿主释放并发起多次停机时提前释放容器
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
