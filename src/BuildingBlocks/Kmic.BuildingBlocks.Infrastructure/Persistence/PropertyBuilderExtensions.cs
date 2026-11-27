using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class PropertyBuilderExtensions
{
    /// <summary>映射为不限长度的文本列（字符串属性须显式设最大长度或调用此方法，仅限 string）</summary>
    public static PropertyBuilder<TProperty> IsUnboundedText<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>启用不区分大小写排序规则（仅限 string；模糊匹配须使用 KmicFunctions 而非直接 ILIKE）</summary>
    public static PropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>按预设配置小数精度（如 Precisions.Amount）</summary>
    public static PropertyBuilder<TProperty> HasPrecision<TProperty>(this PropertyBuilder<TProperty> builder, DecimalPrecision precision)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(precision);
        return builder.HasPrecision(precision.Precision, precision.Scale);
    }

    private static void EnsureString<TProperty>(PropertyBuilder<TProperty> builder, string method)
    {
        if (typeof(TProperty) != typeof(string))
        {
            throw new InvalidOperationException($"{method} 只能用于字符串属性，{builder.Metadata.Name} 的类型是 {typeof(TProperty).Name}。");
        }
    }
}
