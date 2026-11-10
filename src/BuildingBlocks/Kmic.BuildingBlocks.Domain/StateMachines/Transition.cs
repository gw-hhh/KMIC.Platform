using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 一条声明了的合法流转
/// </summary>
internal sealed record Transition<TAggregate, TState, TAction>(TState From, TAction Action, TState To, Func<TAggregate, Result>? Guard)
    where TAggregate : class, IAggregateRoot
    where TState : notnull
    where TAction : notnull
{
    public string FromCode { get; } = StateCodes.Of(From);

    public string ActionCode { get; } = StateCodes.Of(Action);

    public string ToCode { get; } = StateCodes.Of(To);
}
