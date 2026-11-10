using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 状态机的入口
/// </summary>
/// <remarks>
/// 写成非泛型静态类上的泛型方法（同 ImmutableArray.Create）：泛型类型上的静态方法会被 CA1000 拦下
/// </remarks>
public static class StateMachine
{
    /// <summary>
    /// 开始声明一个聚合的状态机；状态属性只能由这里给出的 setter 改写（分析器 KM0401）
    /// </summary>
    /// <param name="state">读取状态属性，如 s =&gt; s.Status；属性名会记入业务历史</param>
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
