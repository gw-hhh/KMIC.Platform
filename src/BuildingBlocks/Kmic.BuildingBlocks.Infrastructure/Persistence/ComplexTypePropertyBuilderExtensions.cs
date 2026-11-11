using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 值对象成员的映射扩展
/// </summary>
public static class ComplexTypePropertyBuilderExtensions
{
    /// <summary>
    /// 不限长度的文本列
    /// </summary>
    /// <exception cref="InvalidOperationException">不是字符串</exception>
    public static ComplexTypePropertyBuilder<TProperty> IsUnboundedText<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>
    /// 不区分大小写
    /// </summary>
    /// <exception cref="InvalidOperationException">不是字符串</exception>
    public static ComplexTypePropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>
    /// 设置小数精度
    /// </summary>
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
