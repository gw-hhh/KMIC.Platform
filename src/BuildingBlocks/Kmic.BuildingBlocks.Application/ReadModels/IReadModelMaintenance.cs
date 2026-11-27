namespace Kmic.BuildingBlocks.Application.ReadModels;

// 读模型重建专用的框架内部读写接口，须在调用方开启的事务内执行，业务模块不应直接调用
internal interface IReadModelMaintenance<T>
    where T : class, IReadModel
{
    Task<long> CountAsync(CancellationToken ct);

    Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(Guid runId, Guid? after, int size, CancellationToken ct);

    Task<int> MarkMissingAsync(IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct);
}
