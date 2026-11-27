using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>状态机构建器。</summary>
public sealed class StateMachineBuilder<TAggregate, TState, TAction>
    where TAggregate : class, IAggregateRoot
    where TState : notnull
    where TAction : notnull
{
    private readonly string _stateName;
    private readonly Func<TAggregate, TState> _getState;
    private readonly Action<TAggregate, TState> _setState;
    private readonly List<Transition<TAggregate, TState, TAction>> _transitions = [];
    // 普通枚举的 default 值本身可能是合法状态，因此单独记录是否显式设置了初始状态
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

    /// <summary>设置初始状态。</summary>
    public StateMachineBuilder<TAggregate, TState, TAction> Initial(TState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _initial = state;
        _hasInitial = true;
        return this;
    }

    /// <summary>设置非法状态流转时返回的默认错误。</summary>
    public StateMachineBuilder<TAggregate, TState, TAction> OnInvalid(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _onInvalid = error;
        return this;
    }

    /// <summary>声明一条状态流转规则。</summary>
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

    /// <summary>校验声明完整性并构建状态机实例。</summary>
    public StateMachine<TAggregate, TState, TAction> Build()
    {
        if (!_hasInitial || _initial is not { } initial)
        {
            throw new InvalidOperationException($"状态机 {MachineName} 尚未调用 Initial 设置初始状态。");
        }

        var onInvalid = _onInvalid ?? throw new InvalidOperationException($"状态机 {MachineName} 尚未调用 OnInvalid 设置默认错误。");

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
                $"状态机 {MachineName} 存在无法从初始状态到达的状态：{string.Join("、", unreachable)}。");
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
