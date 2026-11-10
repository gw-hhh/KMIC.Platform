using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合根基类：持有并发版本，暂存领域事件与状态流转，等事务行为和拦截器取走
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot, ITransitionRecorder
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<StateTransition> _stateTransitions = [];

    /// <summary>
    /// 用已生成的 ID 创建
    /// </summary>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// 只供 EF 物化使用
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// 并发版本：每次保存递增，详情与命令结果都返回给前端，修改时带回比较
    /// </summary>
    public long ConcurrencyVersion { get; private set; }

    /// <summary>
    /// 尚未分发的领域事件
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// 尚未写入业务历史的状态流转
    /// </summary>
    public IReadOnlyList<StateTransition> StateTransitions => _stateTransitions;

    /// <inheritdoc />
    public void ClearPendingRecords() => _domainEvents.Clear();

    /// <inheritdoc />
    public void ClearStateTransitions() => _stateTransitions.Clear();

    void ITransitionRecorder.RecordTransition(StateTransition transition) => _stateTransitions.Add(transition);

    /// <summary>
    /// 产生一个领域事件，在本次命令的事务内分发
    /// </summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
