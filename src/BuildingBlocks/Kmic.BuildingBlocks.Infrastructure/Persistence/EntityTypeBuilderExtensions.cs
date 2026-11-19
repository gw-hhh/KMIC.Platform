using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 实体映射扩展
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// 值对象集合，存为一个 jsonb 列
    /// </summary>
    /// <remarks>
    /// 映射同名的私有字段，如 Contacts 对应 _contacts
    /// </remarks>
    /// <exception cref="InvalidOperationException">找不到对应的私有字段，或字段不是 List&lt;T&gt;</exception>
    public static EntityTypeBuilder<TEntity> ValueObjectCollection<TEntity, TElement>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, IReadOnlyList<TElement>>> property,
        Action<ComplexCollectionBuilder<TElement>>? configure = null)
        where TEntity : class
        where TElement : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(property);

        var name = property.Body is MemberExpression { Member: PropertyInfo member }
            ? member.Name
            : throw new InvalidOperationException("ValueObjectCollection 的参数只能是属性访问，如 e => e.Contacts。");
        var fieldName = $"_{char.ToLowerInvariant(name[0])}{name[1..]}";
        var field = typeof(TEntity).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.FieldType != typeof(List<TElement>))
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name}.{name} 要由私有字段 List<{typeof(TElement).Name}> {fieldName} 支撑，没有找到这个字段。");
        }

        builder.Ignore(name);
        builder.ComplexCollection<List<TElement>, TElement>(fieldName, collection =>
        {
            collection.ToJson(name);
            configure?.Invoke(collection);
        });
        return builder;
    }
}
