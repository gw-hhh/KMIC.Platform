namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 状态流转记录器
/// </summary>
internal interface ITransitionRecorder
{
    void RecordTransition(StateTransition transition);
}
