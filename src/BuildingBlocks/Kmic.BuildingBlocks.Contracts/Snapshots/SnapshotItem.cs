namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 一条快照及其来源版本，读模型按版本比较，只接受更新的版本
/// </summary>
/// <param name="Snapshot">聚合对外公开的完整快照</param>
/// <param name="SourceVersion">快照对应的聚合并发版本</param>
public sealed record SnapshotItem<TSnapshot>(TSnapshot Snapshot, long SourceVersion);
