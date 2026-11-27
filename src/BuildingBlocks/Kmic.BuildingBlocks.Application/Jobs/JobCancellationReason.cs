namespace Kmic.BuildingBlocks.Application.Jobs;

public enum JobCancellationReason
{
    /// <summary>用户取消。</summary>
    UserRequested,

    /// <summary>实例停机。</summary>
    Shutdown,

    /// <summary>超过最长运行时间。</summary>
    Timeout,

    /// <summary>提交人被禁用。</summary>
    SubmitterDisabled,
}
