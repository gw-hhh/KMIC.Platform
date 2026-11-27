namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>快照分页结果。</summary>
/// <param name="Items">本页快照列表</param>
/// <param name="Next">下一页游标，无后续数据时为 null</param>
public sealed record SnapshotPage<TSnapshot>(IReadOnlyList<SnapshotItem<TSnapshot>> Items, Guid? Next);
