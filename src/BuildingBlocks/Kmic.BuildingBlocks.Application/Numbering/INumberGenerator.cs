namespace Kmic.BuildingBlocks.Application.Numbering;

/// <summary>
/// 业务编号的取号入口
/// </summary>
/// <remarks>
/// 计数行加锁到事务结束：在处理器最后一步取号，取号之后不再做耗时操作（05-13 D1）
/// </remarks>
public interface INumberGenerator
{
    /// <summary>
    /// 取下一个编号；只能在命令事务中调用
    /// </summary>
    /// <param name="rule">规则名，即模块中声明的常量</param>
    /// <param name="partition">分组键，只用于分组计数的规则；必须来自字典或智能枚举编码</param>
    /// <param name="cancellationToken">取消令牌</param>
    ValueTask<string> NextAsync(string rule, string? partition = null, CancellationToken cancellationToken = default);
}
