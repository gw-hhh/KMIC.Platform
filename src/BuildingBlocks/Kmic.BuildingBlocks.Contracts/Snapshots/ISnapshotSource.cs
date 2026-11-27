namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>全量快照数据源，包含已停用及已删除的数据（仅限系统身份调用）。</summary>
public interface ISnapshotSource<TSnapshot>
{
    /// <summary>按 ID 分页读取。</summary>
    /// <param name="after">上一页末条记录的 ID，首页传 null</param>
    Task<SnapshotPage<TSnapshot>> GetPageAsync(Guid? after, int size, CancellationToken ct);

    /// <summary>按 ID 批量读取快照，已物理删除的记录不包含在结果中。</summary>
    Task<IReadOnlyDictionary<Guid, SnapshotItem<TSnapshot>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
