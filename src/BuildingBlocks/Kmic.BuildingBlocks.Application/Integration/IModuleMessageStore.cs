namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个模块的消息存储：读写本模块的发件箱与收件箱，给 Operations 的管理视图用
/// </summary>
/// <remarks>
/// 发件箱与收件箱表在各自模块的 schema 里，Operations 通过每个模块的这个存储遍历。这与投递程序一样是框架基础设施的例外，
/// 不是 Operations 读取别的模块的业务数据。所有写入都带状态条件：只改死信，已经处理完、正在处理的行不会被碰
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
    /// <param name="ct">取消令牌</param>
    Task<ModuleMessageBacklog> GetBacklogAsync(CancellationToken ct);

    /// <summary>
    /// 按发生时间倒序取一批死信，时间相同按 ID 倒序
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <param name="after">从这个位置之后取（不含）；第一批不给</param>
    /// <param name="take">最多取几条</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<DeadMessage>> ListDeadAsync(DeadMessageFilter filter, MessagePosition? after, int take, CancellationToken ct);

    /// <summary>
    /// 取一条消息的全部内容，状态在 <see cref="DeadMessageDetail.State" /> 里：死信、已放弃，或者还不是死信；没有这一行时得到空
    /// </summary>
    /// <param name="box">在哪个箱</param>
    /// <param name="id">行 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<DeadMessageDetail?> GetAsync(MessageBox box, Guid id, CancellationToken ct);

    /// <summary>
    /// 被死信挡住的顺序键，等得最多的排前面；最多取 500 个
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<BlockedKey>> ListBlockedKeysAsync(CancellationToken ct);

    /// <summary>
    /// 重试死信：状态回到待处理，次数清零，马上可以被领取
    /// </summary>
    /// <param name="box">在哪个箱</param>
    /// <param name="ids">行 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>每个 ID 一条结果，顺序与传入一致</returns>
    Task<IReadOnlyList<MessageActionResult>> RetryAsync(MessageBox box, IReadOnlyCollection<Guid> ids, CancellationToken ct);

    /// <summary>
    /// 放弃收件箱里的死信：状态改为已放弃，原因记进错误说明。只有收件箱能放弃：发件箱的死信是投递本身出了问题，只能重试
    /// </summary>
    /// <param name="ids">收件箱行 ID</param>
    /// <param name="reason">原因</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>每个 ID 一条结果，顺序与传入一致</returns>
    Task<IReadOnlyList<MessageActionResult>> AbandonAsync(IReadOnlyCollection<Guid> ids, string reason, CancellationToken ct);
}
