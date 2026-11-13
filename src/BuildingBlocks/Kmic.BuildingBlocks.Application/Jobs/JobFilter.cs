namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 列后台作业的筛选条件，给了的条件都要满足
/// </summary>
/// <param name="JobType">作业类型名</param>
/// <param name="State">状态</param>
/// <param name="SubmittedBy">提交人</param>
public sealed record JobFilter(string? JobType = null, BackgroundJobState? State = null, Guid? SubmittedBy = null)
{
    /// <summary>
    /// 不筛选
    /// </summary>
    public static JobFilter None { get; } = new();
}
