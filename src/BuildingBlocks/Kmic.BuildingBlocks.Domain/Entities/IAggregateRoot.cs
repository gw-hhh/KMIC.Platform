using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合根的非泛型接口
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// 并发版本号
    /// </summary>
    long ConcurrencyVersion { get; }

    /// <summary>
    /// 尚未分发的领域事件
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 尚未记录的状态流转
    /// </summary>
    IReadOnlyList<StateTransition> StateTransitions { get; }

    /// <summary>
    /// 清除领域事件
    /// </summary>
    void ClearPendingRecords();

    /// <summary>
    /// 清除状态流转
    /// </summary>
    void ClearStateTransitions();
}
