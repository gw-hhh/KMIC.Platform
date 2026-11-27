namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>读模型写入器，在同步命令处理器中调用。</summary>
public interface IReadModelWriter<in T>
    where T : class, IReadModel
{
    /// <summary>实时同步写入：仅当本地记录非终态且版本更低时更新，返回是否实际写入。</summary>
    Task<bool> UpsertAsync(T row, CancellationToken ct);

    /// <summary>处理源端物理删除快照，将本地记录置为终态；<paramref name="row"/> 仅需填充 ID、来源版本与状态，其余字段填占位值即可。</summary>
    Task<bool> PurgeAsync(T row, CancellationToken ct);

    /// <summary>重建扫描写入：版本不低于本地时覆盖更新并记录轮次，已处终态的记录跳过。</summary>
    Task<bool> UpsertFromScanAsync(T row, Guid runId, CancellationToken ct);
}
