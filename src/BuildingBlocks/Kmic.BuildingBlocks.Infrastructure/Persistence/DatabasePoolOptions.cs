namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public sealed class DatabasePoolOptions
{
    public int TxMaxPoolSize { get; set; }

    public int ReadMaxPoolSize { get; set; }

    /// <summary>流式池上限（仅对 Worker 节点生效，0 表示不创建）</summary>
    public int StreamMaxPoolSize { get; set; }
}
