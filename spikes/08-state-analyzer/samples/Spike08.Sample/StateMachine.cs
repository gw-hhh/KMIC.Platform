namespace Kmic.BuildingBlocks.Domain;

/// <summary>
/// 原型用的状态机签名
/// </summary>
public sealed class StateMachine<TAggregate, TState, TAction>
{
    public StateMachine<TAggregate, TState, TAction> Build() => this;
}
