namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 按 ID 键集分页的一页快照
/// </summary>
/// <param name="Items">本页快照，按 ID 升序</param>
/// <param name="Next">下一页的起点（不含）；为空表示已到末尾</param>
public sealed record SnapshotPage<TSnapshot>(IReadOnlyList<SnapshotItem<TSnapshot>> Items, Guid? Next);
