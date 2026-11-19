using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Infrastructure.Background;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Background;

/// <summary>
/// 内存队列，测试用：领取就是从队列里取，处理交给测试给的委托
/// </summary>
internal sealed class FakeWork(int parallelism, TimeSpan poll) : ILeasedWork<int>
{
    private readonly ConcurrentQueue<int> _queue = new();
    private int _failClaims;
    private long _failedAt;
    private long _claimedAfterFailureAt;

    public ConcurrentQueue<int> Processed { get; } = new();

    public ConcurrentQueue<int> ClaimSizes { get; } = new();

    public Func<int, CancellationToken, Task> Handler { get; set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    /// 失败的领取到其后第一次领到条目之间的间隔；还没有这两次领取时为空
    /// </summary>
    public TimeSpan? DelayAfterFailure
    {
        get
        {
            var failed = Interlocked.Read(ref _failedAt);
            var claimed = Interlocked.Read(ref _claimedAfterFailureAt);
            return failed != 0 && claimed != 0 ? TimeProvider.System.GetElapsedTime(failed, claimed) : null;
        }
    }

    public string Name => "test";

    public int Parallelism => parallelism;

    public TimeSpan PollInterval => poll;

    public void Enqueue(params int[] items)
    {
        foreach (var item in items)
        {
            _queue.Enqueue(item);
        }
    }

    // 接下来的几次领取抛数据库不可用的异常
    public void FailNextClaims(int times) => _failClaims = times;

    public Task<IReadOnlyList<int>> ClaimAsync(int max, Guid token, CancellationToken ct)
    {
        if (_failClaims > 0)
        {
            _failClaims--;
            Interlocked.Exchange(ref _failedAt, TimeProvider.System.GetTimestamp());
            throw new Npgsql.NpgsqlException("数据库连接不上");
        }

        var claimed = new List<int>();
        while (claimed.Count < max && _queue.TryDequeue(out var item))
        {
            claimed.Add(item);
        }

        if (claimed.Count > 0)
        {
            ClaimSizes.Enqueue(claimed.Count);
            if (Interlocked.Read(ref _failedAt) != 0)
            {
                Interlocked.CompareExchange(ref _claimedAfterFailureAt, TimeProvider.System.GetTimestamp(), 0);
            }
        }

        return Task.FromResult<IReadOnlyList<int>>(claimed);
    }

    public async Task ProcessAsync(int item, Guid token, CancellationToken ct)
    {
        try
        {
            await Handler(item, ct);
        }
        finally
        {
            Processed.Enqueue(item);
        }
    }
}
