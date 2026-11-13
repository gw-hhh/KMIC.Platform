using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Background;

/// <summary>
/// 领取队列的后台循环：等启动任务、并行上限、单条失败不停循环、数据库不可用时退避、停机等进行中的处理
/// </summary>
public sealed class LeasedQueueRunnerTests : IAsyncDisposable
{
    private readonly StartupState _startup = new();
    private LeasedQueueRunner<int>? _runner;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask DisposeAsync()
    {
        if (_runner is not null)
        {
            await _runner.StopAsync(CancellationToken.None);
            _runner.Dispose();
        }
    }

    private async Task<FakeWork> StartAsync(int parallelism, TimeSpan poll)
    {
        var queue = new FakeWork(parallelism, poll);
        _runner = new LeasedQueueRunner<int>(queue, _startup, NullLogger<LeasedQueueRunner<int>>.Instance);
        await _runner.StartAsync(Ct);
        return queue;
    }

    [Fact]
    public async Task NothingIsClaimed_UntilTheStartupTasksHaveFinished()
    {
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        queue.Enqueue(1, 2);

        await Task.Delay(TimeSpan.FromMilliseconds(300), TimeProvider.System, Ct);
        queue.Processed.ShouldBeEmpty("启动任务还没完成，不能领取");

        _startup.Complete();
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 2, "启动完成后领取并处理", ct: Ct);
    }

    [Fact]
    public async Task ItemsAreProcessedInParallel_ButNeverMoreThanTheLimit()
    {
        _startup.Complete();
        var queue = await StartAsync(3, TimeSpan.FromMilliseconds(50));
        var running = 0;
        var peak = 0;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.Handler = async (_, ct) =>
        {
            var now = Interlocked.Increment(ref running);
            InterlockedMax(ref peak, now);
            await release.Task.WaitAsync(ct);
            Interlocked.Decrement(ref running);
        };

        queue.Enqueue(1, 2, 3, 4, 5, 6);
        await Eventually.UntilAsync(_ => Task.FromResult(Volatile.Read(ref running)), count => count == 3, "占满三个槽位", ct: Ct);
        await Task.Delay(TimeSpan.FromMilliseconds(200), TimeProvider.System, Ct);
        Volatile.Read(ref running).ShouldBe(3, "第四条要等有槽位空出来才会被领取");

        release.SetResult();
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 6, "全部处理完", ct: Ct);
        Volatile.Read(ref peak).ShouldBe(3);
        queue.ClaimSizes.ShouldAllBe(size => size <= 3, "一次最多领空闲槽位那么多条");
    }

    [Fact]
    public async Task AnItemThatFails_DoesNotStopTheLoop()
    {
        _startup.Complete();
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        queue.Handler = (item, _) => item == 2 ? throw new InvalidOperationException("这一条处理失败") : Task.CompletedTask;

        queue.Enqueue(1, 2, 3);
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 3, "失败的一条之外的也处理了", ct: Ct);

        queue.Enqueue(4);
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 4, "出错之后循环还在", ct: Ct);
    }

    [Fact]
    public async Task WhenTheDatabaseIsUnavailable_ItBacksOffAndPicksUpAgain()
    {
        _startup.Complete();
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        queue.FailNextClaims(1);
        queue.Enqueue(1);

        // 第一次领取失败后至少退避 1 秒，恢复后领取到
        var started = TimeProvider.System.GetTimestamp();
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 1, "数据库恢复后继续", TimeSpan.FromSeconds(10), Ct);
        TimeProvider.System.GetElapsedTime(started).ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(900));
    }

    [Fact]
    public async Task Stopping_WaitsForTheItemsInFlight()
    {
        _startup.Complete();
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.Handler = async (_, _) =>
        {
            started.TrySetResult();
            await release.Task;
        };
        queue.Enqueue(1);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);

        var stopping = _runner!.StopAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(300), TimeProvider.System, Ct);
        stopping.IsCompleted.ShouldBeFalse("进行中的处理没做完，停机要等");

        release.SetResult();
        await stopping.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        queue.Processed.Count.ShouldBe(1);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        do
        {
            current = Volatile.Read(ref target);
            if (value <= current)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref target, value, current) != current);
    }
}
