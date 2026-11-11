namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 端点要遵守的并发限制策略
/// </summary>
/// <param name="Policies">策略名，见 <see cref="ConcurrencyPolicies"/></param>
public sealed record ConcurrencyMetadata(IReadOnlyList<string> Policies);
