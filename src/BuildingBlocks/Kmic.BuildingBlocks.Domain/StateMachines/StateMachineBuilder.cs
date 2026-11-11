using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 状态机声明
/// </summary>
public sealed class StateMachineBuilder<TAggregate, TState, TAction>
    where TAggregate : class, IAggregateRoot
    where TState : notnull
    where TAction : notnull
{
    private readonly string _stateName;
    private readonly Func<TAggregate, TState> _getState;
    private readonly Action<TAggregate, TState> _setState;
    private readonly List<Transition<TAggregate, TState, TAction>> _transitions = [];
    // 普通枚举的默认值也是合法状态，所以另外记录是否设置过
    private TState? _initial;
    private bool _hasInitial;
    private Error? _onInvalid;

    internal StateMachineBuilder(string stateName, Func<TAggregate, TState> getState, Action<TAggregate, TState> setState)
    {
        _stateName = stateName;
        _getState = getState;
        _setState = setState;
    }

    private string MachineName => $"{typeof(TAggregate).Name}.{_stateName}";

    /// <summary>
    /// 设置初始状态
    /// </summary>
    public StateMachineBuilder<TAggregate, TState, TAction> Initial(TState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _initial = state;
        _hasInitial = true;
        return this;
    }

    /// <summary>
    /// 动作不允许时返回的错误
    /// </summary>
    public StateMachineBuilder<TAggregate, TState, TAction> OnInvalid(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _onInvalid = error;
        return this;
    }

    /// <summary>
    /// 声明一条流转
    /// </summary>
    public StateMachineBuilder<TAggregate, TState, TAction> Permit(
        TState from,
        TAction action,
        TState to,
        Func<TAggregate, Result>? guard = null)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(to);
        _transitions.Add(new Transition<TAggregate, TState, TAction>(from, action, to, guard));
        return this;
    }

    /// <summary>
    /// 生成状态机，声明有误时抛异常
    /// </summary>
    public StateMachine<TAggregate, TState, TAction> Build()
    {
        if (!_hasInitial || _initial is not { } initial)
        {
            throw new InvalidOperationException($"状态机 {MachineName} 没有调用 Initial。");
        }

        var onInvalid = _onInvalid ?? throw new InvalidOperationException($"状态机 {MachineName} 没有调用 OnInvalid。");

        var duplicated = _transitions
            .GroupBy(t => (t.FromCode, t.ActionCode))
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicated is not null)
        {
            throw new InvalidOperationException(
                $"状态机 {MachineName} 重复声明了“{duplicated.Key.FromCode} + {duplicated.Key.ActionCode}”。");
        }

        var unreachable = FindUnreachable(StateCodes.Of(initial));
        if (unreachable.Count > 0)
        {
            throw new InvalidOperationException(
                $"状态机 {MachineName} 中这些状态无法从初始状态到达：{string.Join("、", unreachable)}。");
        }

        return new StateMachine<TAggregate, TState, TAction>(
            new StateMachineDefinition<TAggregate, TState, TAction>(_stateName, _getState, _setState, initial, onInvalid, [.. _transitions]));
    }

    private List<string> FindUnreachable(string initialCode)
    {
        var reached = new HashSet<string>(StringComparer.Ordinal) { initialCode };
        var pending = new Queue<string>([initialCode]);
        while (pending.TryDequeue(out var current))
        {
            foreach (var transition in _transitions.Where(t => string.Equals(t.FromCode, current, StringComparison.Ordinal)))
            {
                if (reached.Add(transition.ToCode))
                {
                    pending.Enqueue(transition.ToCode);
                }
            }
        }

        return [.. StateCodes.AllValues<TState>().Select(StateCodes.Of).Where(code => !reached.Contains(code))];
    }
}
