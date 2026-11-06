using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Spike06.Tests;

// 06 第 11.3 节的 .MirrorTo(列)：迁移过渡期，把属性的值同时写入另一列（影子属性），读取只用属性映射的列
public static class MirrorExtensions
{
    public const string Annotation = "Kmic:MirrorTo";

    public static PropertyBuilder<TProperty> MirrorTo<TProperty>(this PropertyBuilder<TProperty> property, string column)
    {
        ArgumentNullException.ThrowIfNull(property);
        var entityType = (IMutableEntityType)property.Metadata.DeclaringType;
        var shadow = entityType.AddProperty($"{property.Metadata.Name}__MirrorTo__{column}", typeof(TProperty));
        shadow.SetColumnName(column);
        shadow.IsNullable = true;
        property.Metadata.SetAnnotation(Annotation, shadow.Name);
        return property;
    }
}
