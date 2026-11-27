using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Locking;

/// <summary>事务级咨询锁；仅可在命令中使用，锁随事务结束自动释放。</summary>
public interface ITransactionLocks
{
    /// <summary>按锁键排序后依次获取共享锁（避免死锁）；等待超时时返回 Common.TransientConflict。</summary>
    Task<Result> AcquireSharedAsync(string scope, IReadOnlyCollection<Guid> ids, TimeSpan timeout, CancellationToken ct);

    /// <summary>尝试获取排他锁并跳过已被持有的 ID，返回成功加锁的 ID 集合。</summary>
    Task<IReadOnlySet<Guid>> TryAcquireExclusiveAsync(string scope, IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
