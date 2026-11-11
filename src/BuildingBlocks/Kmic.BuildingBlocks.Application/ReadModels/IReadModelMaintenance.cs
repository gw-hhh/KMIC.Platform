namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>
/// 重建读模型时框架自己用的读写，模块代码不用
/// </summary>
/// <remarks>
/// 和 IReadModelWriter 由同一个类实现，都要在调用方已经开好的事务里执行
/// </remarks>
internal interface IReadModelMaintenance<T>
    where T : class, IReadModel
{
    /// <summary>
    /// 读模型的总行数，包括终态的行
    /// </summary>
    Task<long> CountAsync(CancellationToken ct);

    /// <summary>
    /// 按 ID 升序列出 after 之后的候选：不是终态，且最近一次扫描到它的不是这一轮
    /// </summary>
    Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(Guid runId, Guid? after, int size, CancellationToken ct);

    /// <summary>
    /// 把源端确认不存在的行置为终态，返回实际改动的行数
    /// </summary>
    /// <remarks>
    /// 只改来源版本还和候选一致的行，期间被实时事件更新过的跳过
    /// </remarks>
    Task<int> MarkMissingAsync(IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct);
}
