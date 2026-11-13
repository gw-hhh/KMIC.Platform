namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 重试或放弃一条消息的结果
/// </summary>
public enum MessageActionOutcome
{
    /// <summary>
    /// 已经做了
    /// </summary>
    Done = 0,

    /// <summary>
    /// 这个模块的这个箱里没有这一行
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// 这一行不是死信：已经处理完、正在处理，或者早就放弃了，没有改它
    /// </summary>
    WrongState = 2,
}
