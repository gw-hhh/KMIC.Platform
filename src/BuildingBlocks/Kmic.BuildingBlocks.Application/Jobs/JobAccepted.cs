namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业已受理
/// </summary>
/// <param name="JobId">作业 ID</param>
public sealed record JobAccepted(Guid JobId);
