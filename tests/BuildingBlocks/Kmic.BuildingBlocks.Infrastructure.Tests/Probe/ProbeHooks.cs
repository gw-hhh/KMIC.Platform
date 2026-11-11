namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 测试控制处理器行为与观察调用次数的开关；每个工具实例一个
/// </summary>
internal sealed class ProbeHooks
{
    private Func<CancellationToken, Task>? _beforeReturn;

    public int Handled { get; set; }

    public int AfterCommitRuns { get; set; }

    public bool FailClosedEvent { get; set; }

    /// <summary>
    /// 处理器返回之前执行的动作，如在另一个连接上修改同一行、等待另一个请求
    /// </summary>
    public void OnBeforeReturn(Func<CancellationToken, Task>? action) => _beforeReturn = action;

    public Task BeforeReturnAsync(CancellationToken ct) => _beforeReturn?.Invoke(ct) ?? Task.CompletedTask;
}
