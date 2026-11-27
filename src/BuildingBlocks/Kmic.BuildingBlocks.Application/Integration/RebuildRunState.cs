namespace Kmic.BuildingBlocks.Application.Integration;

public enum RebuildRunState
{
    /// <summary>排队或进行中。</summary>
    Active,

    /// <summary>源端缺失行数超过阈值，暂停等待运维确认。</summary>
    AwaitingConfirmation,

    Completed,

    Abandoned,

    Failed,
}
