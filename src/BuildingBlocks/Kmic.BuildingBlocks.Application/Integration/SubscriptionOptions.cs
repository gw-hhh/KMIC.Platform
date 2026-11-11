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

    /// <summary>
    /// 重建读模型时拉取快照的接口
    /// </summary>
    public Type? RebuildSource { get; private set; }

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
    /// 订阅第一次登记或运维要求时，从这个接口拉取全部快照重建读模型
    /// </summary>
    public SubscriptionOptions RebuildWith<TSource>()
        where TSource : class
    {
        RebuildSource = typeof(TSource);
        return this;
    }

    internal static bool IsSnapshotSource(Type type) =>
        type.IsInterface && (IsSnapshotSourceType(type) || type.GetInterfaces().Any(IsSnapshotSourceType));

    private static bool IsSnapshotSourceType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISnapshotSource<>);
}
