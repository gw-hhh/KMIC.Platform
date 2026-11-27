using Kmic.BuildingBlocks.Application.ReadModels;

namespace Kmic.BuildingBlocks.Application.Integration;

internal interface IReadModelRebuild
{
    Type ReadModel { get; }

    Type Source { get; }

    Task<RebuildPage> GetPageAsync(IServiceProvider services, Guid? after, int size, CancellationToken ct);

    Task<IReadOnlyList<IReadModel>> GetRowsAsync(IServiceProvider services, IReadOnlyCollection<Guid> ids, CancellationToken ct);

    // 写入重建扫描行；若目标行已处于终态则跳过更新并返回 false
    Task<bool> ScanAsync(IServiceProvider services, IReadModel row, Guid runId, CancellationToken ct);

    Task<long> CountAsync(IServiceProvider services, CancellationToken ct);

    Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(IServiceProvider services, Guid runId, Guid? after, int size, CancellationToken ct);

    Task<int> MarkMissingAsync(IServiceProvider services, IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct);
}
