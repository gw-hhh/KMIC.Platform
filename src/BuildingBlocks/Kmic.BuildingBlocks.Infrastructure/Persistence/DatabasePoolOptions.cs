namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 连接池上限
/// </summary>
public sealed class DatabasePoolOptions
{
    /// <summary>
    /// 事务池上限
    /// </summary>
    public int TxMaxPoolSize { get; set; }

    /// <summary>
    /// 读池上限
    /// </summary>
    public int ReadMaxPoolSize { get; set; }

    /// <summary>
    /// 流式池上限，只对 Worker 有效，为 0 时不建
    /// </summary>
    public int StreamMaxPoolSize { get; set; }
}
