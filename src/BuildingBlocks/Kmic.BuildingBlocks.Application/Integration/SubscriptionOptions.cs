using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.Snapshots;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 订阅的可选设置
/// </summary>
public sealed class SubscriptionOptions
{
    /// <summary>
    /// 是否按顺序键依次处理
    /// </summary>
    public bool IsOrdered { get; private set; }

    internal IReadModelRebuild? Rebuild { get; private set; }

    /// <summary>
    /// 同一个顺序键的消息按发布顺序处理
    /// </summary>
    /// <remarks>
    /// 前一条进入死信时，同一个键后面的消息都会等着，要及时处理死信
    /// </remarks>
    public SubscriptionOptions Ordered()
    {
        IsOrdered = true;
        return this;
    }

    /// <summary>
    /// 订阅第一次登记或运维要求时，从源模块拉取全部快照重建读模型
    /// </summary>
    /// <remarks>
    /// 映射要和这个订阅的同步命令用同一份，免得实时同步和重建写出不一样的行
    /// </remarks>
    /// <typeparam name="TSource">源模块提供的快照来源接口</typeparam>
    /// <typeparam name="TSnapshot">快照类型</typeparam>
    /// <typeparam name="TRow">本模块的读模型</typeparam>
    /// <param name="toRow">把快照转成读模型的一行</param>
    public SubscriptionOptions RebuildWith<TSource, TSnapshot, TRow>(Func<SnapshotItem<TSnapshot>, TRow> toRow)
        where TSource : class, ISnapshotSource<TSnapshot>
        where TRow : class, IReadModel
    {
        ArgumentNullException.ThrowIfNull(toRow);
        Rebuild = new ReadModelRebuild<TSource, TSnapshot, TRow>(toRow);
        return this;
    }
}
