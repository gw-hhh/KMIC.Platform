using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class ComplexTypePropertyBuilderExtensions
{
    /// <summary>映射为不限长度的文本列（仅限 string 成员）</summary>
    public static ComplexTypePropertyBuilder<TProperty> IsUnboundedText<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>启用不区分大小写排序规则（仅限 string 成员）</summary>
    public static ComplexTypePropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>按预设配置小数精度</summary>
    public static ComplexTypePropertyBuilder<TProperty> HasPrecision<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, DecimalPrecision precision)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(precision);
        return builder.HasPrecision(precision.Precision, precision.Scale);
    }

    private static void EnsureString<TProperty>(ComplexTypePropertyBuilder<TProperty> builder, string method)
    {
        if (typeof(TProperty) != typeof(string))
        {
            throw new InvalidOperationException($"{method} 只能用于字符串成员，{builder.Metadata.Name} 的类型是 {typeof(TProperty).Name}。");
        }
    }
}
