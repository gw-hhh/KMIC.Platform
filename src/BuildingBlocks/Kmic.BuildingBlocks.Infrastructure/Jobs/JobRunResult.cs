namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 执行一个作业的结果
/// </summary>
internal enum JobRunResult
{
    /// <summary>
    /// 成功
    /// </summary>
    Succeeded,

    /// <summary>
    /// 失败，原因写在作业行上
    /// </summary>
    Failed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 停机时可续跑的作业放回队列，等别的实例续跑
    /// </summary>
    Released,

    /// <summary>
    /// 占用已被别的执行者接管，本地停止，不写任何结果
    /// </summary>
    TakenOver,

    /// <summary>
    /// 某一批的提交结果未知，本地停止，等占用到期后由领取流程处理
    /// </summary>
    OutcomeUnknown,
}
