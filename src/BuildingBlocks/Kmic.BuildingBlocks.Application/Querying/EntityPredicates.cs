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
    /// <remarks>
    /// 泛型参数没有 == 运算符，所以手写表达式树；ID 放在闭包对象的属性上，EF 会把它当参数，不同 ID 共用同一份编译结果
    /// </remarks>
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
