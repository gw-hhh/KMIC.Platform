namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 让状态机把流转记录写进聚合根，而业务代码无法直接写入
/// </summary>
internal interface ITransitionRecorder
{
    void RecordTransition(StateTransition transition);
}
