using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Echo.Probe;

public sealed class EchoHooks
{
    /// <summary>按单据标题指定模拟失败的条目，每条仅失败一次</summary>
    public IDictionary<string, Error> Failures { get; } = new Dictionary<string, Error>(StringComparer.Ordinal);

    /// <summary>剩余需要抛出异常的次数</summary>
    public int Throws { get; set; }

    /// <summary>处理器返回前执行的回调（如在另一连接接管消息）</summary>
    public Func<CancellationToken, Task>? BeforeReturn { get; set; }
}
