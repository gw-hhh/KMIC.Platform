using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

// ILeasedWork 领取循环（仅在 Worker 角色运行）
internal sealed class LeasedQueueRunner<TItem>(ILeasedWork<TItem> queue, StartupState startup, ILogger<LeasedQueueRunner<TItem>> logger) : BackgroundService
{
    private static readonly TimeSpan _maxDatabaseDelay = TimeSpan.FromSeconds(60);

    private readonly AsyncSignal _signal = new();
    private readonly ConcurrentDictionary<Guid, Task> _running = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startup.WaitAsync(stoppingToken);

        using var slots = new SemaphoreSlim(queue.Parallelism);
        var failures = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var free = await EnterAsync(slots, stoppingToken);
            if (free == 0)
            {
                continue;
            }

            var token = Guid.CreateVersion7();
            IReadOnlyList<TItem> claimed;
            try
            {
                claimed = await queue.ClaimAsync(free, token, stoppingToken);
                failures = 0;
            }
            catch (DbException exception)
            {
                slots.Release(free);
                failures++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, failures - 1), _maxDatabaseDelay.TotalSeconds));
                LeasedQueueLog.DatabaseUnavailable(logger, exception, queue.Name, delay.TotalSeconds);
                await _signal.WaitAsync(delay, stoppingToken);
                continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                slots.Release(free);
                break;
            }

            if (claimed.Count < free)
            {
                slots.Release(free - claimed.Count);
            }

            if (claimed.Count == 0)
            {
                await _signal.WaitAsync(queue.PollInterval, stoppingToken);
                continue;
            }

            foreach (var item in claimed)
            {
                var id = Guid.CreateVersion7();
                _running[id] = RunAsync(id, item, token, slots, stoppingToken);
            }
        }

        // 停机时停止领取新任务，等待进行中的任务处理完成
        await Task.WhenAll(_running.Values);
    }

    // 等待并批量占用当前所有空闲槽位，超时或停机返回 0
    private async Task<int> EnterAsync(SemaphoreSlim slots, CancellationToken stopping)
    {
        try
        {
            if (!await slots.WaitAsync(queue.PollInterval, stopping))
            {
                return 0;
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            return 0;
        }

        var taken = 1;
        while (taken < queue.Parallelism && await slots.WaitAsync(TimeSpan.Zero, CancellationToken.None))
        {
            taken++;
        }

        return taken;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "单条处理失败不应中断整个领取循环，异常记录为错误日志")]
    private async Task RunAsync(Guid id, TItem item, Guid token, SemaphoreSlim slots, CancellationToken stopping)
    {
        try
        {
            await queue.ProcessAsync(item, token, stopping);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            // 停机取消后，待占用过期即可由其他实例重新领取
        }
        catch (Exception exception)
        {
            LeasedQueueLog.ProcessingFailed(logger, exception, queue.Name);
        }
        finally
        {
            slots.Release();
            _running.TryRemove(id, out _);
        }
    }
}
