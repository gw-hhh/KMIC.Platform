namespace Kmic.BuildingBlocks.Application.Integration;

public enum MessageActionOutcome
{
    Done = 0,

    /// <summary>指定模块的消息箱中不存在该记录。</summary>
    NotFound = 1,

    /// <summary>非死信状态，未作变更。</summary>
    WrongState = 2,
}
