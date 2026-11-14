namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 读模型重建的一轮
/// </summary>
/// <param name="Module">订阅方模块</param>
/// <param name="RunId">轮次 ID</param>
/// <param name="Subscription">订阅名</param>
/// <param name="ReadModel">读模型类型名</param>
/// <param name="JobId">执行这一轮的后台作业</param>
/// <param name="State">状态</param>
/// <param name="Scanned">已扫描的行数</param>
/// <param name="Missing">源端确认不存在的行数，暂停等确认时是已发现的数量</param>
/// <param name="StartedAt">开始时间</param>
/// <param name="FinishedAt">结束时间</param>
public sealed record RebuildRunInfo(
    string Module,
    Guid RunId,
    string Subscription,
    string ReadModel,
    Guid JobId,
    RebuildRunState State,
    long Scanned,
    long Missing,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt);
