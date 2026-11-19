namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>
/// 读模型写入
/// </summary>
/// <remarks>
/// 在同步命令的处理器里调用
/// </remarks>
public interface IReadModelWriter<in T>
    where T : class, IReadModel
{
    /// <summary>
    /// 实时同步：本地行不是终态、版本更低时才写入，返回是否写入
    /// </summary>
    Task<bool> UpsertAsync(T row, CancellationToken ct);

    /// <summary>
    /// 收到物理删除的快照：置为终态
    /// </summary>
    /// <remarks>
    /// row 只需要 ID、来源版本和状态，其余字段写占位值
    /// </remarks>
    Task<bool> PurgeAsync(T row, CancellationToken ct);

    /// <summary>
    /// 重建扫描：版本相同也覆盖并记下轮次；本地版本更高时只记轮次。终态行不修改，返回 false
    /// </summary>
    Task<bool> UpsertFromScanAsync(T row, Guid runId, CancellationToken ct);
}
