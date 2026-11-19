namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 作业成功后的结果
/// </summary>
/// <param name="ResultFileId">结果文件</param>
/// <param name="Summary">结果摘要</param>
public sealed record JobOutcome(Guid? ResultFileId = null, object? Summary = null);
