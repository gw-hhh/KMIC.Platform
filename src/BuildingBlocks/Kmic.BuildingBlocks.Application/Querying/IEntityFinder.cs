using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询处理器按 ID 取单个对象
/// </summary>
/// <remarks>
/// 不存在、已删除、在数据范围之外，对外都是同一个 NotFound，不让调用方探测别人的数据是否存在。
/// 区别只在内部：因数据范围看不到时，另外记一条安全事件
/// </remarks>
public interface IEntityFinder
{
    /// <summary>
    /// 取出 ID 对应的对象，取不到时返回 <paramref name="notFound" />
    /// </summary>
    /// <remarks>
    /// 只有取不到时才多查一次是否被数据范围挡住，正常路径没有额外开销。
    /// 查询里的 Include 等写法照常生效，补查不受影响
    /// </remarks>
    /// <param name="query">从模块查询入口出发的查询</param>
    /// <param name="id">要找的 ID</param>
    /// <param name="notFound">取不到时返回的错误，如 SampleErrors.NotFound</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result<TEntity>> FindByIdOrNotFoundAsync<TEntity, TId>(
        IQueryable<TEntity> query, TId id, Error notFound, CancellationToken cancellationToken)
        where TEntity : Entity<TId>
        where TId : struct, IStronglyTypedId;
}
