namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 读模型重建一轮的状态
/// </summary>
public enum RebuildRunState
{
    /// <summary>
    /// 排队或进行中
    /// </summary>
    Active,

    /// <summary>
    /// 源端找不到的行太多，暂停等运维确认
    /// </summary>
    AwaitingConfirmation,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 运维放弃了这一轮
    /// </summary>
    Abandoned,

    /// <summary>
    /// 作业失败或被取消
    /// </summary>
    Failed,
}
