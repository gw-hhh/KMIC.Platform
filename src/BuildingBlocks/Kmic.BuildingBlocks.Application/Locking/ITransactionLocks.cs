using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>
/// 按对象 ID 取数据库的事务级咨询锁：互斥由数据库保证，不靠时钟
/// </summary>
/// <remarks>
/// 只能在命令里用，锁随命令的事务提交或回滚释放；PostgreSQL 先让提交对别的会话可见、再释放锁，
/// 所以业务停在提交阶段时锁仍然持有。咨询锁的键空间是整个数据库共用的，键由 <see cref="LockKeys" /> 派生，
/// 哈希碰撞只会造成多余的等待或一次跳过，不影响正确性
/// </remarks>
public interface ITransactionLocks
{
    /// <summary>
    /// 对每个 ID 取共享锁；按锁键排序后依次取，避免两个命令交叉等待
    /// </summary>
    /// <returns>成功，或等得太久时的暂时冲突（Common.TransientConflict，可重试）</returns>
    Task<Result> AcquireSharedAsync(string scope, IReadOnlyCollection<Guid> ids, TimeSpan timeout, CancellationToken ct);

    /// <summary>
    /// 对每个 ID 尝试取排他锁，取不到的不等待、直接跳过
    /// </summary>
    /// <remarks>
    /// 给清理这类“可以下次再做”的一方用：有业务命令持有共享锁的对象这次不处理，不去和业务抢
    /// </remarks>
    /// <returns>取到锁的 ID；其余的被别人持有，调用方这次不要动它们</returns>
    Task<IReadOnlySet<Guid>> TryAcquireExclusiveAsync(string scope, IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
