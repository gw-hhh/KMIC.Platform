namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 作业被取消的原因
/// </summary>
/// <remarks>
/// 停机时可续跑的作业可以先保存检查点；其余原因直接结束即可
/// </remarks>
public enum JobCancellationReason
{
    /// <summary>
    /// 用户取消
    /// </summary>
    UserRequested,

    /// <summary>
    /// 实例停机
    /// </summary>
    Shutdown,

    /// <summary>
    /// 超过最长运行时间
    /// </summary>
    Timeout,

    /// <summary>
    /// 提交人被禁用
    /// </summary>
    SubmitterDisabled,
}
