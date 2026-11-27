using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>聚合根基类。</summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot, ITransitionRecorder
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<StateTransition> _stateTransitions = [];

    /// <summary>使用既有 ID 初始化聚合根。</summary>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>仅供 EF Core 实例化使用。</summary>
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

    public void ClearPendingRecords() => _domainEvents.Clear();

    public void ClearStateTransitions() => _stateTransitions.Clear();

    void ITransitionRecorder.RecordTransition(StateTransition transition) => _stateTransitions.Add(transition);

    /// <summary>触发并暂存领域事件。</summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
