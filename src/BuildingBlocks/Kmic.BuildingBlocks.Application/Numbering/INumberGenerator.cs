namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 业务编号生成
/// </summary>
/// <remarks>
/// 取号会锁住计数到事务结束，所以放在处理器最后一步，取号后不要再做耗时操作
/// </remarks>
public interface INumberGenerator
{
    /// <summary>
    /// 取下一个编号，只能在命令事务里调用
    /// </summary>
    /// <param name="rule">规则名</param>
    /// <param name="partition">分组键，只有分组计数的规则才传，取字典或枚举编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    ValueTask<string> NextAsync(string rule, string? partition = null, CancellationToken cancellationToken = default);
}
