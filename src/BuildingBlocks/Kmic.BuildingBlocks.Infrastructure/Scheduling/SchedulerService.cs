using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// Worker 角色里运行调度器
/// </summary>
/// <remarks>
/// 启动任务全部完成后才开始触发。停机时等正在执行的命令结束，宿主的停机超时兜底；
/// 没注册它的实例上调度器一直处于待机，只用来手动执行和改时间
/// </remarks>
internal sealed class SchedulerService(ISchedulerFactory schedulers, StartupState startup) : BackgroundService
{
    private int _stopped;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        // 停机可能被调用两次（测试宿主释放时会再停一次）：调度器关过之后就取不到了，第二次直接返回
        if (Interlocked.Exchange(ref _stopped, 1) == 1)
        {
            return;
        }

        var scheduler = await schedulers.GetScheduler(cancellationToken);
        if (scheduler.Status is SchedulerStatus.Running or SchedulerStatus.Standby)
        {
            await scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startup.WaitAsync(stoppingToken);
        var scheduler = await schedulers.GetScheduler(stoppingToken);
        await scheduler.Start(stoppingToken);
    }
}
