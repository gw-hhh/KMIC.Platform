using System.Linq.Expressions;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 按 ID 过滤用的表达式
/// </summary>
public static class EntityPredicates
{
    /// <summary>
    /// ID 等于给定值
    /// </summary>
    public static Expression<Func<TEntity, bool>> HasId<TEntity, TId>(TId id)
        where TEntity : Entity<TId>
        where TId : struct, IStronglyTypedId
    {
        var entity = Expression.Parameter(typeof(TEntity), "e");
        var value = Expression.Property(Expression.Constant(new IdHolder<TId>(id)), nameof(IdHolder<>.Value));
        return Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(Expression.Property(entity, nameof(Entity<>.Id)), value), entity);
    }

    private sealed record IdHolder<TId>(TId Value);
}
