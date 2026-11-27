namespace Kmic.BuildingBlocks.Domain.StateMachines;

internal interface ITransitionRecorder
{
    void RecordTransition(StateTransition transition);
}
