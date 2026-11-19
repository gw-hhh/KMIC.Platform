using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询处理器按 ID 取单个对象
/// </summary>
/// <remarks>
/// 不存在、已删除、在数据范围外都返回同一个 NotFound
/// </remarks>
public interface IEntityFinder
{
    /// <summary>
    /// 取出 ID 对应的对象，取不到时返回 <paramref name="notFound" />
    /// </summary>
    Task<Result<TEntity>> FindByIdOrNotFoundAsync<TEntity, TId>(
        IQueryable<TEntity> query, TId id, Error notFound, CancellationToken cancellationToken)
        where TEntity : Entity<TId>
        where TId : struct, IStronglyTypedId;
}
