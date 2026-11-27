namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 重建作业断点（After 为下一页起始游标）
internal sealed record RebuildCheckpoint(bool ScanFinished, Guid? After, long Scanned);
