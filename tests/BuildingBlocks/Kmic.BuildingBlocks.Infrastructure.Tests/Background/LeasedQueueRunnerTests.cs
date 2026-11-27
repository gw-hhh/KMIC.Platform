using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Background;

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
        queue.Processed.ShouldBeEmpty("启动任务尚未完成前不应领取条目");

        _startup.Complete();
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 2, "启动任务完成后领取并处理条目", ct: Ct);
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
        await Eventually.UntilAsync(_ => Task.FromResult(Volatile.Read(ref running)), count => count == 3, "占用全部三个并发槽位", ct: Ct);
        await Task.Delay(TimeSpan.FromMilliseconds(200), TimeProvider.System, Ct);
        Volatile.Read(ref running).ShouldBe(3, "第四条需等待空闲槽位释放后才能被领取");

        release.SetResult();
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 6, "全部条目处理完成", ct: Ct);
        Volatile.Read(ref peak).ShouldBe(3);
        queue.ClaimSizes.ShouldAllBe(size => size <= 3, "单次领取数量不应超过当前空闲槽位数");
    }

    [Fact]
    public async Task AnItemThatFails_DoesNotStopTheLoop()
    {
        _startup.Complete();
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        queue.Handler = (item, _) => item == 2 ? throw new InvalidOperationException("这一条处理失败") : Task.CompletedTask;

        queue.Enqueue(1, 2, 3);
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 3, "除失败条目外的其余条目均已处理", ct: Ct);

        queue.Enqueue(4);
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 4, "发生异常后后台循环仍继续处理新条目", ct: Ct);
    }

    [Fact]
    public async Task WhenTheDatabaseIsUnavailable_ItBacksOffAndPicksUpAgain()
    {
        _startup.Complete();
        var queue = await StartAsync(2, TimeSpan.FromMilliseconds(50));
        queue.FailNextClaims(1);
        queue.Enqueue(1);

        // 首次领取失败后至少退避 1 秒，待数据库恢复后重新领取。间隔在测试替身内部按两次领取时间戳计算，不受测试线程调度影响
        await Eventually.UntilAsync(_ => Task.FromResult(queue.Processed.Count), count => count == 1, "数据库恢复后继续领取并处理条目", TimeSpan.FromSeconds(10), Ct);
        queue.DelayAfterFailure.ShouldNotBeNull().ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(900));
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
        stopping.IsCompleted.ShouldBeFalse("存在进行中的任务时，停机应等待其完成");

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
