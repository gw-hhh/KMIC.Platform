namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 测试用来控制处理器行为、统计调用次数的开关
/// </summary>
internal sealed class ProbeHooks
{
    private Func<CancellationToken, Task>? _beforeReturn;

    public int Handled { get; set; }

    public int AfterCommitRuns { get; set; }

    public bool FailClosedEvent { get; set; }

    /// <summary>
    /// 处理器返回前执行的动作，如在另一个连接上改同一行、等待另一个请求
    /// </summary>
    public void OnBeforeReturn(Func<CancellationToken, Task>? action) => _beforeReturn = action;

    public Task BeforeReturnAsync(CancellationToken ct) => _beforeReturn?.Invoke(ct) ?? Task.CompletedTask;

    /// <summary>
    /// 作业在第几批（从 0 开始）返回失败
    /// </summary>
    public int? JobFailAtBatch { get; set; }

    /// <summary>
    /// 作业在第几批（从 0 开始）抛出异常
    /// </summary>
    public int? JobThrowAtBatch { get; set; }

    /// <summary>
    /// 作业每一批开始前执行，参数是批次序号
    /// </summary>
    public Func<int, CancellationToken, Task>? BeforeJobBatch { get; set; }
}
