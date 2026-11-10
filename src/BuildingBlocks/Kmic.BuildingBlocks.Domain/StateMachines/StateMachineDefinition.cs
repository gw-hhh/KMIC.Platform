using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// Build 检查通过后的完整声明
/// </summary>
internal sealed record StateMachineDefinition<TAggregate, TState, TAction>(
    string StateName,
    Func<TAggregate, TState> GetState,
    Action<TAggregate, TState> SetState,
    TState Initial,
    Error OnInvalid,
    IReadOnlyList<Transition<TAggregate, TState, TAction>> Transitions)
    where TAggregate : class, IAggregateRoot
    where TState : notnull
    where TAction : notnull;
