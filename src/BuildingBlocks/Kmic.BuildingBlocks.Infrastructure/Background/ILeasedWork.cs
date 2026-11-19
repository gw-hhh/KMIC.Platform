namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// “表就是队列”的队列：模块只写怎样领取和处理一条，循环由框架提供
/// </summary>
/// <remarks>
/// 完成、失败、重新排队的写入都要带“令牌仍是本次领取”的条件
/// </remarks>
/// <typeparam name="TItem">领取到的一条</typeparam>
public interface ILeasedWork<TItem>
{
    /// <summary>
    /// 队列的名字，写日志用
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 同时处理的条数上限
    /// </summary>
    int Parallelism { get; }

    /// <summary>
    /// 没有被唤醒时，隔多久查一次队列
    /// </summary>
    TimeSpan PollInterval { get; }

    /// <summary>
    /// 领取最多 <paramref name="max" /> 条，没有就返回空列表
    /// </summary>
    Task<IReadOnlyList<TItem>> ClaimAsync(int max, Guid token, CancellationToken ct);

    /// <summary>
    /// 处理一条；失败要自己记录结果（退避、转人工），抛出来的异常只会被记成错误日志
    /// </summary>
    Task ProcessAsync(TItem item, Guid token, CancellationToken ct);
}
