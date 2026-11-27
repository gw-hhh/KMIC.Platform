namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>终结订阅的结果；<paramref name="Unfinished"/> 为尚未处理完成的消息条数。</summary>
public sealed record RouteFinalizeResult(RouteAdminOutcome Outcome, long Unfinished);
