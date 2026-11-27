namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>后台作业按类型汇总的排队概况。</summary>
public sealed record JobBacklog(string Module, string Type, long Queued, TimeSpan OldestQueued);
