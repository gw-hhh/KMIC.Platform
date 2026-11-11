using Kmic.BuildingBlocks.Application.ReadModels;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 订阅声明的读模型重建方式，把快照类型、来源接口和读模型的泛型参数藏在里面，重建作业用不着知道它们
/// </summary>
/// <remarks>
/// 方法里的 services 是重建作业当前的作用域
/// </remarks>
internal interface IReadModelRebuild
{
    /// <summary>
    /// 读模型的类型
    /// </summary>
    Type ReadModel { get; }

    /// <summary>
    /// 快照来源的接口类型
    /// </summary>
    Type Source { get; }

    /// <summary>
    /// 从源端读一页快照
    /// </summary>
    Task<RebuildPage> GetPageAsync(IServiceProvider services, Guid? after, int size, CancellationToken ct);

    /// <summary>
    /// 按 ID 向源端确认，返回仍然存在的行
    /// </summary>
    Task<IReadOnlyList<IReadModel>> GetRowsAsync(IServiceProvider services, IReadOnlyCollection<Guid> ids, CancellationToken ct);

    /// <summary>
    /// 重建扫描写入一行，终态的行不修改，返回 false
    /// </summary>
    Task<bool> ScanAsync(IServiceProvider services, IReadModel row, Guid runId, CancellationToken ct);

    Task<long> CountAsync(IServiceProvider services, CancellationToken ct);

    Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(IServiceProvider services, Guid runId, Guid? after, int size, CancellationToken ct);

    Task<int> MarkMissingAsync(IServiceProvider services, IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct);
}
