using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合根基类
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
    /// 只供 EF 使用
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// 并发版本号
    /// </summary>
    public long ConcurrencyVersion { get; private set; }

    /// <summary>
    /// 尚未分发的领域事件
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// 尚未记录的状态流转
    /// </summary>
    public IReadOnlyList<StateTransition> StateTransitions => _stateTransitions;

    /// <inheritdoc />
    public void ClearPendingRecords() => _domainEvents.Clear();

    /// <inheritdoc />
    public void ClearStateTransitions() => _stateTransitions.Clear();

    void ITransitionRecorder.RecordTransition(StateTransition transition) => _stateTransitions.Add(transition);

    /// <summary>
    /// 发布领域事件
    /// </summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
