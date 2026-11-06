namespace Kmic.BuildingBlocks.Domain;

/// <summary>
/// 替代写法：非泛型静态类上的泛型方法
/// </summary>
public static class StateMachine
{
    public static StateMachine<TAggregate, TState, TAction> Define<TAggregate, TState, TAction>(
        Func<TAggregate, TState> getter,
        Action<TAggregate, TState> setter) => new();
}
