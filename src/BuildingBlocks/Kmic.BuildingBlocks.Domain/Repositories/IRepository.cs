using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Repositories;

/// <summary>聚合根仓储接口；记录不存在、已软删除或无权访问时均返回 NotFound 错误。</summary>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>按 ID 读取聚合根以供更新。</summary>
    Task<Result<TAggregate>> GetForUpdateAsync(TId id, long? expectedVersion, CancellationToken ct);

    /// <summary>按 ID 读取并加行级排他锁（持有至当前事务结束）。</summary>
    Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct);

    void Add(TAggregate aggregate);

    void Remove(TAggregate aggregate);
}
