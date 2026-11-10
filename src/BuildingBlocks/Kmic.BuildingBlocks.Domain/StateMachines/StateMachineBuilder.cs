using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 声明初始状态、非法流转的错误与全部合法流转，Build 时检查声明是否完整
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
    // 状态可能是普通枚举（值类型），默认值本身就是一个合法状态，所以单独记录是否声明过
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
    /// 初始状态，由 Start 设置
    /// </summary>
    public StateMachineBuilder<TAggregate, TState, TAction> Initial(TState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _initial = state;
        _hasInitial = true;
        return this;
    }

    /// <summary>
    /// 当前状态不允许该动作时返回的错误，必须来自本模块的错误目录
    /// </summary>
    public StateMachineBuilder<TAggregate, TState, TAction> OnInvalid(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _onInvalid = error;
        return this;
    }

    /// <summary>
    /// 声明一条合法流转；守卫失败时原样返回守卫的错误，前端可以得到具体原因
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
    /// 检查声明并生成状态机；声明有误即抛出异常，第一次单元测试就会暴露
    /// </summary>
    public StateMachine<TAggregate, TState, TAction> Build()
    {
        if (!_hasInitial || _initial is not { } initial)
        {
            throw new InvalidOperationException($"状态机 {MachineName} 没有声明初始状态（Initial）。");
        }

        var onInvalid = _onInvalid ?? throw new InvalidOperationException($"状态机 {MachineName} 没有声明非法流转的错误（OnInvalid）。");

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
                $"状态机 {MachineName} 中以下状态从初始状态无法到达：{string.Join("、", unreachable)}。");
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
