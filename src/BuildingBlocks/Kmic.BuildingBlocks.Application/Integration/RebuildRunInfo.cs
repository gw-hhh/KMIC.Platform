namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>读模型重建轮次信息；<paramref name="Missing"/> 为源端确认不存在的行数。</summary>
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
