using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>
/// 按对象 ID 取事务级咨询锁
/// </summary>
/// <remarks>
/// 只能在命令里用，锁随事务结束释放
/// </remarks>
public interface ITransactionLocks
{
    /// <summary>
    /// 对每个 ID 取共享锁；按锁键排序后依次取，避免两个命令交叉等待
    /// </summary>
    /// <returns>成功，或等得太久时的暂时冲突（Common.TransientConflict，可重试）</returns>
    Task<Result> AcquireSharedAsync(string scope, IReadOnlyCollection<Guid> ids, TimeSpan timeout, CancellationToken ct);

    /// <summary>
    /// 对每个 ID 尝试取排他锁，取不到的跳过
    /// </summary>
    /// <returns>取到锁的 ID；其余的被别人持有，调用方这次不要动它们</returns>
    Task<IReadOnlySet<Guid>> TryAcquireExclusiveAsync(string scope, IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
