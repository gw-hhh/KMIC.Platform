namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业列表筛选条件，已指定的条件须同时满足。</summary>
/// <param name="JobType">作业类型名</param>
/// <param name="State">状态</param>
/// <param name="SubmittedBy">提交人</param>
public sealed record JobFilter(string? JobType = null, BackgroundJobState? State = null, Guid? SubmittedBy = null)
{
    public static JobFilter None { get; } = new();
}
