namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>端点并发限制策略元数据</summary>
public sealed record ConcurrencyMetadata(IReadOnlyList<string> Policies);
