namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>业务编号生成器；取号会锁定计数器直至事务结束，应放在处理器最后一步执行。</summary>
public interface INumberGenerator
{
    /// <summary>生成下一个编号，仅可在命令事务中调用。</summary>
    ValueTask<string> NextAsync(string rule, string? partition = null, CancellationToken ct = default);
}
