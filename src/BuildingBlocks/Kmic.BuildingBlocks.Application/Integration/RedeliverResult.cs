namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>消息补投结果；<paramref name="Candidates"/> 为指定区间内符合条件的消息数。</summary>
public sealed record RedeliverResult(RouteAdminOutcome Outcome, long Candidates, long Written);
