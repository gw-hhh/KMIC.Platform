namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业受理凭据。</summary>
/// <param name="Module">作业所属模块，查询进度或取消时的路径格式为 my-jobs/{模块}/{作业 ID}。</param>
public sealed record JobAccepted(Guid JobId, string Module);
