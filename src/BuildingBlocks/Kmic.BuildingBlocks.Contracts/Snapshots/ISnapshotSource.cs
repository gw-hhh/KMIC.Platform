namespace Kmic.BuildingBlocks.Contracts.Snapshots;

/// <summary>
/// 源模块提供的全量快照，供订阅方初始化与重建读模型，不依赖历史消息
/// </summary>
/// <remarks>
/// 结果含已删除、已停用的数据；只允许系统身份调用（ContractAccess.SystemOnly，启动校验）
/// </remarks>
public interface ISnapshotSource<TSnapshot>
{
    /// <summary>
    /// 按 ID 键集分页读取
    /// </summary>
    /// <param name="after">上一页的末尾 ID（不含）；从头开始时为空</param>
    /// <param name="size">每页条数</param>
    /// <param name="ct">取消令牌</param>
    Task<SnapshotPage<TSnapshot>> GetPageAsync(Guid? after, int size, CancellationToken ct);

    /// <summary>
    /// 供重建收尾确认：已物理删除的 ID 不出现在结果中
    /// </summary>
    Task<IReadOnlyDictionary<Guid, SnapshotItem<TSnapshot>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
