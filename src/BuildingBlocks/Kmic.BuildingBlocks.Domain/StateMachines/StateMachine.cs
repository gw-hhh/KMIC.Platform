using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 状态机
/// </summary>
/// <remarks>
/// 泛型方法放在非泛型类上，避开 CA1000
/// </remarks>
public static class StateMachine
{
    /// <summary>
    /// 声明状态机
    /// </summary>
    /// <param name="state">状态属性，如 s =&gt; s.Status</param>
    /// <param name="setState">写入状态属性，如 (s, v) =&gt; s.Status = v</param>
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
            throw new ArgumentException("state 必须是直接读取一个属性的表达式，如 s => s.Status。", nameof(state));
        }

        return new StateMachineBuilder<TAggregate, TState, TAction>(stateName, state.Compile(), setState);
    }
}
