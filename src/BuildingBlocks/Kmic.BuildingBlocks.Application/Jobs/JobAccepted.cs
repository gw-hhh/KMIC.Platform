namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业已受理
/// </summary>
/// <remarks>
/// 做成后台作业的命令返回它，接口层转成 202 并带上作业资源的地址
/// </remarks>
/// <param name="JobId">作业 ID，用它查询进度和结果</param>
public sealed record JobAccepted(Guid JobId);
