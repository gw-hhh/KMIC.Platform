using System.Collections.Concurrent;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ProbeHooks
{
    private Func<CancellationToken, Task>? _beforeReturn;

    public int Handled { get; set; }

    public int AfterCommitRuns { get; set; }

    public bool FailClosedEvent { get; set; }

    // 处理器返回前执行（如在另一连接上修改同行或等待并发请求）
    public void OnBeforeReturn(Func<CancellationToken, Task>? action) => _beforeReturn = action;

    public Task BeforeReturnAsync(CancellationToken ct) => _beforeReturn?.Invoke(ct) ?? Task.CompletedTask;

    public int? JobFailAtBatch { get; set; }

    public int? JobThrowAtBatch { get; set; }

    public Func<int, CancellationToken, Task>? BeforeJobBatch { get; set; }

    public ConcurrentQueue<DateTimeOffset> ScheduledFires { get; } = new();

    public ConcurrentQueue<string> Exported { get; } = new();

    public TicketId? StampTicket { get; set; }

    public bool FailScheduled { get; set; }
}
