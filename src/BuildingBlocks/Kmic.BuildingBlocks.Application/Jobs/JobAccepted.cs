namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业已受理
/// </summary>
/// <param name="JobId">作业 ID</param>
/// <param name="Module">作业所在的模块，查进度和取消时放进地址：my-jobs/{模块}/{作业 ID}</param>
public sealed record JobAccepted(Guid JobId, string Module);
