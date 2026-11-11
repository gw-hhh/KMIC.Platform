using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Echo.Probe;

/// <summary>
/// 测试控制处理器的行为
/// </summary>
public sealed class EchoHooks
{
    /// <summary>
    /// 按单据标题指定失败，每个只失败一次
    /// </summary>
    public IDictionary<string, Error> Failures { get; } = new Dictionary<string, Error>(StringComparer.Ordinal);

    /// <summary>
    /// 还要抛出异常的次数
    /// </summary>
    public int Throws { get; set; }

    /// <summary>
    /// 处理器返回前执行，比如在另一个连接上接管这条消息
    /// </summary>
    public Func<CancellationToken, Task>? BeforeReturn { get; set; }
}
