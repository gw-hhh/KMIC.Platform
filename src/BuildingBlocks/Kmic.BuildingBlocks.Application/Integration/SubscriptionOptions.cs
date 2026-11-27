using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.Snapshots;

namespace Kmic.BuildingBlocks.Application.Integration;

public sealed class SubscriptionOptions
{
    public bool IsOrdered { get; private set; }

    internal IReadModelRebuild? Rebuild { get; private set; }

    /// <summary>启用顺序消费，同一顺序键的消息按发布顺序依次处理；前驱进入死信时后续消息将阻塞等待。</summary>
    public SubscriptionOptions Ordered()
    {
        IsOrdered = true;
        return this;
    }

    /// <summary>配置从源模块全量拉取快照重建读模型的策略；快照映射逻辑须与增量同步命令保持一致。</summary>
    public SubscriptionOptions RebuildWith<TSource, TSnapshot, TRow>(Func<SnapshotItem<TSnapshot>, TRow> toRow)
        where TSource : class, ISnapshotSource<TSnapshot>
        where TRow : class, IReadModel
    {
        ArgumentNullException.ThrowIfNull(toRow);
        Rebuild = new ReadModelRebuild<TSource, TSnapshot, TRow>(toRow);
        return this;
    }
}
