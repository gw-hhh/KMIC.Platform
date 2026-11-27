namespace Kmic.BuildingBlocks.Application.Integration;

public enum DeadMessageState
{
    /// <summary>死信状态，支持重试或放弃。</summary>
    Dead = 0,

    /// <summary>已被运维放弃，不再处理。</summary>
    Abandoned = 1,

    /// <summary>非死信状态（待处理、处理中或已处理完成）。</summary>
    Other = 2,
}
