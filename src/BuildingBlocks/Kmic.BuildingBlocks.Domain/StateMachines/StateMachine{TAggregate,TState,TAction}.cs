using System.Text;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 一个聚合的状态机：执行流转、产生业务历史所需的流转记录、回答当前可用的动作
/// </summary>
/// <remarks>
/// 声明为聚合的 private static readonly 字段（_machine），所有实例共用；状态机本身不保存任何聚合的数据
/// </remarks>
public sealed class StateMachine<TAggregate, TState, TAction>
    where TAggregate : class, IAggregateRoot
    where TState : notnull
    where TAction : notnull
{
    private readonly StateMachineDefinition<TAggregate, TState, TAction> _definition;
    private readonly Dictionary<(string FromCode, string ActionCode), Transition<TAggregate, TState, TAction>> _byKey;

    internal StateMachine(StateMachineDefinition<TAggregate, TState, TAction> definition)
    {
        _definition = definition;
        _byKey = definition.Transitions.ToDictionary(t => (t.FromCode, t.ActionCode));
    }

    /// <summary>
    /// 在工厂方法中调用：设置初始状态，并记录一条原状态为空的流转，业务历史由此从创建开始完整
    /// </summary>
    public void Start(TAggregate aggregate, string? reason)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        _definition.SetState(aggregate, _definition.Initial);
        Record(aggregate, null, StateCodes.Of(_definition.Initial), StateTransition.CreateActionCode, reason);
    }

    /// <summary>
    /// 执行一个动作：当前状态不允许时返回 OnInvalid 的错误，守卫不通过时返回守卫的错误，两种情况状态都不变
    /// </summary>
    /// <remarks>
    /// 不发布领域事件：流转是否需要通知其他部分是业务决定，由聚合方法显式 Raise
    /// </remarks>
    public Result Fire(TAggregate aggregate, TAction action, string? reason)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(action);

        var found = Find(aggregate, action);
        if (found.IsFailure)
        {
            return found;
        }

        var transition = found.Value;
        _definition.SetState(aggregate, transition.To);
        Record(aggregate, transition.FromCode, transition.ToCode, transition.ActionCode, reason);
        return Result.Success();
    }

    /// <summary>
    /// 判断当前能否执行该动作，不改变状态
    /// </summary>
    public Result CanFire(TAggregate aggregate, TAction action)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(action);

        var found = Find(aggregate, action);
        return found.IsSuccess ? Result.Success() : found.Error;
    }

    /// <summary>
    /// 当前状态下声明了且守卫通过的全部动作，按声明顺序；allowedActions 由它与资源策略的结果取交集
    /// </summary>
    public IReadOnlyList<TAction> PermittedActions(TAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var currentCode = CurrentCode(aggregate);
        return
        [
            .. _definition.Transitions
                .Where(t => string.Equals(t.FromCode, currentCode, StringComparison.Ordinal))
                .Where(t => t.Guard is null || t.Guard(aggregate).IsSuccess)
                .Select(t => t.Action),
        ];
    }

    /// <summary>
    /// 导出 Mermaid 状态图；模块的文档测试用它保证文档与代码一致
    /// </summary>
    public string ToMermaid()
    {
        var builder = new StringBuilder();
        builder.Append("stateDiagram-v2\n");
        builder.Append("    [*] --> ").Append(StateCodes.Of(_definition.Initial)).Append('\n');
        foreach (var transition in _definition.Transitions)
        {
            builder.Append("    ").Append(transition.FromCode).Append(" --> ").Append(transition.ToCode)
                .Append(" : ").Append(transition.ActionCode).Append('\n');
        }

        return builder.ToString();
    }

    private Result<Transition<TAggregate, TState, TAction>> Find(TAggregate aggregate, TAction action)
    {
        if (!_byKey.TryGetValue((CurrentCode(aggregate), StateCodes.Of(action)), out var transition))
        {
            return _definition.OnInvalid;
        }

        var guarded = transition.Guard?.Invoke(aggregate);
        return guarded is { IsFailure: true } ? guarded.Error : transition;
    }

    private string CurrentCode(TAggregate aggregate)
    {
        // 状态属性为空只可能是工厂方法漏调了 Start，属于程序缺陷
        var current = _definition.GetState(aggregate)
            ?? throw new InvalidOperationException($"{typeof(TAggregate).Name}.{_definition.StateName} 尚未设置，工厂方法中应先调用 Start。");
        return StateCodes.Of(current);
    }

    private void Record(TAggregate aggregate, string? fromCode, string toCode, string actionCode, string? reason)
    {
        if (aggregate is not ITransitionRecorder recorder)
        {
            throw new InvalidOperationException($"{typeof(TAggregate).Name} 必须继承 AggregateRoot<TId>。");
        }

        int? revision = aggregate is IHasRevision hasRevision ? hasRevision.Revision : null;
        recorder.RecordTransition(new StateTransition(_definition.StateName, fromCode, toCode, actionCode, reason, revision));
    }
}
