namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>模块发件箱与收件箱运维接口；所有状态变更均须附带状态条件，仅允许操作死信记录。</summary>
public interface IModuleMessageStore
{
    string Module { get; }

    /// <summary>查询发件箱及各收件箱订阅的积压、死信与阻塞键汇总。</summary>
    Task<ModuleMessageBacklog> GetBacklogAsync(CancellationToken ct);

    /// <summary>按发生时间倒序分页查询死信，时间相同时按 ID 倒序。</summary>
    Task<IReadOnlyList<DeadMessage>> ListDeadAsync(DeadMessageFilter filter, MessagePosition? after, int take, CancellationToken ct);

    /// <summary>查询单条消息详情。</summary>
    Task<DeadMessageDetail?> GetAsync(MessageBox box, Guid id, CancellationToken ct);

    /// <summary>查询被死信阻塞的顺序键。</summary>
    Task<IReadOnlyList<BlockedKey>> ListBlockedKeysAsync(CancellationToken ct);

    /// <summary>重试死信，按传入顺序返回每条处理结果。</summary>
    Task<IReadOnlyList<MessageActionResult>> RetryAsync(MessageBox box, IReadOnlyCollection<Guid> ids, CancellationToken ct);

    /// <summary>放弃收件箱死信并记录原因，按传入顺序返回每条处理结果。</summary>
    Task<IReadOnlyList<MessageActionResult>> AbandonAsync(IReadOnlyCollection<Guid> ids, string reason, CancellationToken ct);
}
