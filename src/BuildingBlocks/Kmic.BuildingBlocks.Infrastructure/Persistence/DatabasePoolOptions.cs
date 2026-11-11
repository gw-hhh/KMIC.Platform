namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 一组连接池的上限
/// </summary>
public sealed class DatabasePoolOptions
{
    /// <summary>
    /// 事务池上限：写入上下文使用，一个命令最多占一个连接
    /// </summary>
    public int TxMaxPoolSize { get; set; }

    /// <summary>
    /// 读池上限：查询上下文使用，包括命令执行期间的嵌套查询
    /// </summary>
    public int ReadMaxPoolSize { get; set; }

    /// <summary>
    /// 流式池上限：只对 Worker 有效，供导出的主查询边读边写；为 0 时不建流式池
    /// </summary>
    public int StreamMaxPoolSize { get; set; }
}
