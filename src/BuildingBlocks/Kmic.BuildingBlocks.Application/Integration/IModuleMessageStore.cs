namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个模块的发件箱和收件箱，给 Operations 的管理视图用
/// </summary>
/// <remarks>
/// 写入都要带状态条件，只改死信
/// </remarks>
public interface IModuleMessageStore
{
    /// <summary>
    /// 模块名
    /// </summary>
    string Module { get; }

    /// <summary>
    /// 积压汇总：发件箱，以及收件箱里每个订阅的积压、死信、被挡住的键
    /// </summary>
    Task<ModuleMessageBacklog> GetBacklogAsync(CancellationToken ct);

    /// <summary>
    /// 按发生时间倒序取一批死信，时间相同按 ID 倒序
    /// </summary>
    Task<IReadOnlyList<DeadMessage>> ListDeadAsync(DeadMessageFilter filter, MessagePosition? after, int take, CancellationToken ct);

    /// <summary>
    /// 取一条消息的全部内容，状态在 <see cref="DeadMessageDetail.State" /> 里：死信、已放弃，或者还不是死信；没有这一行时得到空
    /// </summary>
    Task<DeadMessageDetail?> GetAsync(MessageBox box, Guid id, CancellationToken ct);

    /// <summary>
    /// 被死信挡住的顺序键，等得最多的排前面；最多取 500 个
    /// </summary>
    Task<IReadOnlyList<BlockedKey>> ListBlockedKeysAsync(CancellationToken ct);

    /// <summary>
    /// 重试死信：状态回到待处理，次数清零，马上可以被领取
    /// </summary>
    /// <returns>每个 ID 一条结果，顺序与传入一致</returns>
    Task<IReadOnlyList<MessageActionResult>> RetryAsync(MessageBox box, IReadOnlyCollection<Guid> ids, CancellationToken ct);

    /// <summary>
    /// 放弃收件箱里的死信：状态改为已放弃，原因记进错误说明。只有收件箱能放弃：发件箱的死信是投递本身出了问题，只能重试
    /// </summary>
    /// <returns>每个 ID 一条结果，顺序与传入一致</returns>
    Task<IReadOnlyList<MessageActionResult>> AbandonAsync(IReadOnlyCollection<Guid> ids, string reason, CancellationToken ct);
}
