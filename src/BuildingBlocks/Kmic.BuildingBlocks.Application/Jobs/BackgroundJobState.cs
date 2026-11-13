namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的状态，数值与作业表里存的一致
/// </summary>
public enum BackgroundJobState
{
    /// <summary>
    /// 排队中
    /// </summary>
    Queued = 0,

    /// <summary>
    /// 运行中
    /// </summary>
    Running = 1,

    /// <summary>
    /// 成功
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已请求取消，执行器还没停
    /// </summary>
    Cancelling = 4,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 5,
}
