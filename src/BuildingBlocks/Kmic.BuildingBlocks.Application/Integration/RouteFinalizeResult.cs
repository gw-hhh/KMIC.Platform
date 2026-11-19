namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 终结订阅的结果
/// </summary>
/// <param name="Outcome">结果</param>
/// <param name="Unfinished">没处理完的消息条数</param>
public sealed record RouteFinalizeResult(RouteAdminOutcome Outcome, long Unfinished);
