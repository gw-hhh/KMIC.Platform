namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 一页快照
/// </summary>
/// <param name="Items">本页快照，按 ID 升序</param>
/// <param name="Next">下一页的起点，没有下一页时为空</param>
public sealed record SnapshotPage<TSnapshot>(IReadOnlyList<SnapshotItem<TSnapshot>> Items, Guid? Next);
