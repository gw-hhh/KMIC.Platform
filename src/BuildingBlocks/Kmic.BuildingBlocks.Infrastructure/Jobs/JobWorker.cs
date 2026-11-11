using System.Collections.Concurrent;
using System.Data.Common;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// Worker 角色里的作业执行
/// </summary>
/// <remarks>
/// 每个模块一个领取循环，整个实例同时运行的作业数有上限。停机时不再领取，
/// 进行中的作业收到停机的取消：可续跑的放回队列，其余标记中断
/// </remarks>
internal sealed class JobWorker(
    JobExecutor executor,
    ModuleFrameworkTables tables,
    MessagingSignals signals,
    StartupState startup,
    IOptions<JobOptions> options,
    IOptions<MessagingOptions> messaging,
    ILogger<JobWorker> logger) : BackgroundService
{
    private static readonly TimeSpan _maxDatabaseDelay = TimeSpan.FromSeconds(60);

    private readonly ConcurrentDictionary<Guid, Task> _running = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startup.WaitAsync(stoppingToken);

        using var slots = new SemaphoreSlim(options.Value.Parallelism);
        await Task.WhenAll(tables.Modules.Select(module => ClaimLoopAsync(module, slots, stoppingToken)));

        // 停机时等进行中的作业放回队列或标记中断，宿主的停机超时兜底
        await Task.WhenAll(_running.Values);
    }

    private async Task ClaimLoopAsync(string module, SemaphoreSlim slots, CancellationToken stopping)
    {
        var signal = signals.For(MessagingChannels.Jobs, module);
        var failures = 0;
        while (!stopping.IsCancellationRequested)
        {
            if (!await TryEnterAsync(slots, stopping))
            {
                continue;
            }

            var token = Guid.CreateVersion7();
            IReadOnlyList<ClaimedJob> claimed;
            try
            {
                claimed = await executor.ClaimAsync(module, 1, token, stopping);
                failures = 0;
            }
            catch (DbException exception)
            {
                slots.Release();
                failures++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, failures - 1), _maxDatabaseDelay.TotalSeconds));
                MessagingLog.LoopDatabaseUnavailable(logger, exception, delay.TotalSeconds);
                await signal.WaitAsync(delay, stopping);
                continue;
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                slots.Release();
                return;
            }

            if (claimed.Count == 0)
            {
                slots.Release();
                await signal.WaitAsync(messaging.Value.PollInterval, stopping);
                continue;
            }

            var job = claimed[0];
            _running[job.Id] = RunAsync(module, job, token, slots, stopping);
        }
    }

    private async Task RunAsync(string module, ClaimedJob job, Guid token, SemaphoreSlim slots, CancellationToken stopping)
    {
        try
        {
            await executor.RunAsync(module, job, token, stopping);
        }
        finally
        {
            slots.Release();
            _running.TryRemove(job.Id, out _);
        }
    }

    private async Task<bool> TryEnterAsync(SemaphoreSlim slots, CancellationToken stopping)
    {
        try
        {
            return await slots.WaitAsync(messaging.Value.PollInterval, stopping);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            return false;
        }
    }
}
