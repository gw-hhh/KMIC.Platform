namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 全量快照来源
/// </summary>
/// <remarks>
/// 包含已删除、已停用的数据，只允许系统身份调用
/// </remarks>
public interface ISnapshotSource<TSnapshot>
{
    /// <summary>
    /// 按 ID 分页读取
    /// </summary>
    /// <param name="after">上一页最后一条的 ID，从头读时为空</param>
    /// <param name="size">每页条数</param>
    /// <param name="ct">取消令牌</param>
    Task<SnapshotPage<TSnapshot>> GetPageAsync(Guid? after, int size, CancellationToken ct);

    /// <summary>
    /// 按 ID 读取，已物理删除的不在结果里
    /// </summary>
    Task<IReadOnlyDictionary<Guid, SnapshotItem<TSnapshot>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
