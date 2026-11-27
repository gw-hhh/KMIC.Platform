namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业成功完成后的结果文件与摘要。</summary>
public sealed record JobOutcome(Guid? ResultFileId = null, object? Summary = null);
