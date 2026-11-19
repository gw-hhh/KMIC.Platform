namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 业务编号生成
/// </summary>
/// <remarks>
/// 取号会锁住计数到事务结束，放在处理器最后一步
/// </remarks>
public interface INumberGenerator
{
    /// <summary>
    /// 取下一个编号，只能在命令事务里调用
    /// </summary>
    ValueTask<string> NextAsync(string rule, string? partition = null, CancellationToken ct = default);
}
