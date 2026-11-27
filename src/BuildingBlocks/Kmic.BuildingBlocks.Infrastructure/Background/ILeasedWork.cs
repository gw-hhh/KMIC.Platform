namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>基于数据库表的占用队列，由框架驱动领取循环</summary>
/// <remarks>完成、失败与重新排队的写入均须校验 token 仍归属当前领取</remarks>
public interface ILeasedWork<TItem>
{
    /// <summary>队列名称（用于日志）</summary>
    string Name { get; }

    /// <summary>并发处理上限</summary>
    int Parallelism { get; }

    /// <summary>无唤醒信号时的轮询间隔</summary>
    TimeSpan PollInterval { get; }

    /// <summary>领取最多 max 条待处理项</summary>
    Task<IReadOnlyList<TItem>> ClaimAsync(int max, Guid token, CancellationToken ct);

    /// <summary>处理单条记录（业务失败需自行记录退避或转人工）</summary>
    Task ProcessAsync(TItem item, Guid token, CancellationToken ct);
}
