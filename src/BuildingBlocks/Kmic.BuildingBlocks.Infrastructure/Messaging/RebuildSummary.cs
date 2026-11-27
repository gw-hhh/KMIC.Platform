namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 重建作业结果摘要（MarkedGone 为已标记消失数，Missing 为收尾时源端缺失总数）
internal sealed record RebuildSummary(long Scanned, long MarkedGone, long Missing, bool AwaitingConfirmation);
