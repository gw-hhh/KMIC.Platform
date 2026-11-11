namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 快照及其来源版本号
/// </summary>
/// <param name="Snapshot">聚合对外公开的完整快照</param>
/// <param name="SourceVersion">来源版本号</param>
public sealed record SnapshotItem<TSnapshot>(TSnapshot Snapshot, long SourceVersion);
