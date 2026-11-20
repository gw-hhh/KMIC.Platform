namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 消息写入的容量保护状态
/// </summary>
public enum AdmissionState
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 软线：拒绝来自 HTTP 与后台作业的消息写入
    /// </summary>
    Soft = 1,

    /// <summary>
    /// 硬线：拒绝全部消息写入
    /// </summary>
    Hard = 2,
}
