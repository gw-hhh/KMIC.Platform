using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// 一个 <see cref="ILeasedWork{TItem}" /> 的领取循环，只在 Worker 角色上运行
/// </summary>
/// <remarks>
/// 启动任务全部完成之前不领取。同时处理的条数不超过队列声明的并行度，一次领取最多领空闲的槽位数
/// </remarks>
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

        // 停机：不再领取，等进行中的处理收尾
        await Task.WhenAll(_running.Values);
    }

    // 等到至少有一个空闲槽位，并把当前所有空闲的槽位一起占下，返回占了几个；等不到或停机返回 0
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "一条处理失败不能让整个循环停下来，异常记成错误日志")]
    private async Task RunAsync(Guid id, TItem item, Guid token, SemaphoreSlim slots, CancellationToken stopping)
    {
        try
        {
            await queue.ProcessAsync(item, token, stopping);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            // 停机时被取消：领取到期后会被重新领取
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
