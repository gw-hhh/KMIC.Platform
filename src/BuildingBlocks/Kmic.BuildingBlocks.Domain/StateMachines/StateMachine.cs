using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>状态机定义入口。</summary>
public static class StateMachine
{
    /// <summary>声明状态机规则。</summary>
    public static StateMachineBuilder<TAggregate, TState, TAction> Define<TAggregate, TState, TAction>(
        Expression<Func<TAggregate, TState>> state,
        Action<TAggregate, TState> setState)
        where TAggregate : class, IAggregateRoot
        where TState : notnull
        where TAction : notnull
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(setState);
        StateCodes.EnsureSupported<TState>();
        StateCodes.EnsureSupported<TAction>();

        if (state.Body is not MemberExpression { Member.Name: var stateName })
        {
            throw new ArgumentException("state 必须为直接访问聚合根属性的表达式，例如 s => s.Status。", nameof(state));
        }

        return new StateMachineBuilder<TAggregate, TState, TAction>(stateName, state.Compile(), setState);
    }
}
