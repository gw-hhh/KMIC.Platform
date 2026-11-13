namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一条消息当前的状态，只区分运维关心的几种
/// </summary>
public enum DeadMessageState
{
    /// <summary>
    /// 死信，可以重试或放弃
    /// </summary>
    Dead = 0,

    /// <summary>
    /// 已被运维放弃，不再处理
    /// </summary>
    Abandoned = 1,

    /// <summary>
    /// 还不是死信：待处理、处理中或已经处理完
    /// </summary>
    Other = 2,
}
