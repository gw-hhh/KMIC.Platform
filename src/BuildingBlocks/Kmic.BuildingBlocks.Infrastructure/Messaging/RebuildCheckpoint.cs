namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 重建作业的检查点
/// </summary>
/// <param name="ScanFinished">源端的快照是否已经扫完，扫完后只剩收尾</param>
/// <param name="After">下一页从哪个 ID 之后开始，收尾阶段不用</param>
/// <param name="Scanned">已经扫描的行数</param>
internal sealed record RebuildCheckpoint(bool ScanFinished, Guid? After, long Scanned);
