namespace Kmic.BuildingBlocks.Infrastructure.Background;

/// <summary>
/// 一个“表就是队列”的队列：模块只写怎样领取、怎样处理一条，循环由框架提供
/// </summary>
/// <remarks>
/// 模块在 AddWorkerServices 里用 AddKmicLeasedWork 登记，框架在 Worker 角色上为它运行领取循环：
/// 有空闲槽位就领取（最多领空闲槽位那么多条），每条在自己的任务里处理；没有可领取的就等唤醒或兜底轮询。
/// 领取要写下领取令牌和到期时间，处理完成、失败、重新排队的写入都要带“令牌仍是本次领取”的条件，
/// 这些由实现的 SQL 负责；处理在数据库事务之外进行，可以调用对象存储、外部接口。
/// 一条处理失败只影响这一条，由实现决定怎么记录和退避；领取时数据库不可用按 1、2、4…最长 60 秒退避。
/// 停机时不再领取，等进行中的处理收尾，宿主的停机超时兜底。同一个条目类型只能有一个队列
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
    /// <param name="max">最多领几条，等于现在空闲的槽位数</param>
    /// <param name="token">本次领取的令牌，写进领取记录，之后的写入要带它</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<TItem>> ClaimAsync(int max, Guid token, CancellationToken ct);

    /// <summary>
    /// 处理一条；失败要自己记录结果（退避、转人工），抛出来的异常只会被记成错误日志
    /// </summary>
    /// <param name="item">领取到的一条</param>
    /// <param name="token">领取时的令牌</param>
    /// <param name="ct">取消令牌，停机时取消</param>
    Task ProcessAsync(TItem item, Guid token, CancellationToken ct);
}
