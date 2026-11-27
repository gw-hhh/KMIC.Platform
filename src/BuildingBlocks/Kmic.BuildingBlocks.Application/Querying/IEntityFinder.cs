using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>查询处理器专用的按 ID 查找器；记录不存在、已删除或超出数据范围时统一返回同一 NotFound 错误。</summary>
public interface IEntityFinder
{
    /// <summary>按 ID 查找实体，不存在时返回 <paramref name="notFound" />。</summary>
    Task<Result<TEntity>> FindByIdOrNotFoundAsync<TEntity, TId>(
        IQueryable<TEntity> query, TId id, Error notFound, CancellationToken cancellationToken)
        where TEntity : Entity<TId>
        where TId : struct, IStronglyTypedId;
}
