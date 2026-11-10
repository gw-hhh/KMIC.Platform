using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合根的非泛型视图，供拦截器、事务行为与架构测试使用
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// 并发版本，由保存时的拦截器递增
    /// </summary>
    long ConcurrencyVersion { get; }

    /// <summary>
    /// 尚未分发的领域事件
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 尚未写入业务历史的状态流转
    /// </summary>
    IReadOnlyList<StateTransition> StateTransitions { get; }

    /// <summary>
    /// 清除已取出的领域事件；状态流转由保存时的拦截器另行清除
    /// </summary>
    void ClearPendingRecords();

    /// <summary>
    /// 清除已写入业务历史的状态流转
    /// </summary>
    void ClearStateTransitions();
}
