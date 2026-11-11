namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 重建作业的结果摘要
/// </summary>
/// <param name="Scanned">扫描的行数</param>
/// <param name="MarkedGone">源端确认已不存在、置为终态的行数</param>
/// <param name="Missing">收尾时源端找不到的行数，暂停时是已发现的数量</param>
/// <param name="AwaitingConfirmation">是否因为找不到的行太多而暂停</param>
internal sealed record RebuildSummary(long Scanned, long MarkedGone, long Missing, bool AwaitingConfirmation);
