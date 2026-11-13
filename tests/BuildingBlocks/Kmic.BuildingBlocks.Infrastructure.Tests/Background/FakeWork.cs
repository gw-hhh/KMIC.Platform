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

    public ConcurrentQueue<int> Processed { get; } = new();

    public ConcurrentQueue<int> ClaimSizes { get; } = new();

    public Func<int, CancellationToken, Task> Handler { get; set; } = (_, _) => Task.CompletedTask;

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
