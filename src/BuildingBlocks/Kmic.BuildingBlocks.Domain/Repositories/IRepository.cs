using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Repositories;

/// <summary>
/// 仓储
/// </summary>
/// <remarks>
/// 不存在、已删除或没有权限时都返回 NotFound
/// </remarks>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// 按 ID 读取，用于修改
    /// </summary>
    Task<Result<TAggregate>> GetForUpdateAsync(TId id, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 按 ID 读取并锁住这一行
    /// </summary>
    /// <remarks>
    /// 锁到事务结束
    /// </remarks>
    Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 新增
    /// </summary>
    void Add(TAggregate aggregate);

    /// <summary>
    /// 删除
    /// </summary>
    void Remove(TAggregate aggregate);
}
