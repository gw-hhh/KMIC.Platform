namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业状态，枚举值与作业表存储一致。</summary>
public enum BackgroundJobState
{
    Queued = 0,

    Running = 1,

    Succeeded = 2,

    Failed = 3,

    /// <summary>已请求取消，执行器尚未停止。</summary>
    Cancelling = 4,

    Cancelled = 5,
}
