using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class EntityTypeBuilderExtensions
{
    /// <summary>将值对象集合映射为单个 jsonb 列（自动绑定同约定私有 List&lt;T&gt; 字段，如 Contacts 对应 _contacts）</summary>
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
            : throw new InvalidOperationException("ValueObjectCollection 的参数必须是属性访问表达式，例如 e => e.Contacts。");
        var fieldName = $"_{char.ToLowerInvariant(name[0])}{name[1..]}";
        var field = typeof(TEntity).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.FieldType != typeof(List<TElement>))
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name}.{name} 须由私有字段 List<{typeof(TElement).Name}> {fieldName} 支撑，未找到匹配的字段。");
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
