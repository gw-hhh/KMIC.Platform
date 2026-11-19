namespace Kmic.BuildingBlocks.Contracts.Jobs;

/// <summary>
/// 后台作业的结果
/// </summary>
public enum BackgroundJobResult
{
    /// <summary>
    /// 成功
    /// </summary>
    Succeeded,

    /// <summary>
    /// 失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,
}
