using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Repositories;

/// <summary>
/// 聚合的仓储：只按标识加载完整聚合并登记新增与删除，查询走读侧
/// </summary>
/// <remarks>
/// 加载不到（不存在、已删除、在数据范围之外）一律返回 NotFound，不区分原因
/// </remarks>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// 加载用于修改的聚合；给出期望版本时先比较，不一致直接返回版本冲突
    /// </summary>
    Task<Result<TAggregate>> GetForUpdateAsync(TId id, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 先对聚合根行加锁（持有到事务结束）再加载，用于多个请求争抢同一聚合的场景
    /// </summary>
    Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct);

    /// <summary>
    /// 登记新增，随事务保存
    /// </summary>
    void Add(TAggregate aggregate);

    /// <summary>
    /// 登记删除，随事务保存（软删除由拦截器改写）
    /// </summary>
    void Remove(TAggregate aggregate);
}
