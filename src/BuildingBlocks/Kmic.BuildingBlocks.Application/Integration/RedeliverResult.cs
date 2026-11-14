namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 补投的结果
/// </summary>
/// <param name="Outcome">结果</param>
/// <param name="Candidates">区间里符合条件的已投递消息条数</param>
/// <param name="Written">真正写进订阅收件箱的条数，订阅已经收到过的不再写</param>
public sealed record RedeliverResult(RouteAdminOutcome Outcome, long Candidates, long Written);
