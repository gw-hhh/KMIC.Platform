using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 值对象（复杂类型）成员的框架映射扩展，含义与 PropertyBuilderExtensions 相同
/// </summary>
public static class ComplexTypePropertyBuilderExtensions
{
    /// <summary>
    /// 长文本，映射为 text
    /// </summary>
    /// <exception cref="InvalidOperationException">成员不是字符串</exception>
    public static ComplexTypePropertyBuilder<TProperty> IsUnboundedText<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>
    /// 不区分大小写
    /// </summary>
    /// <exception cref="InvalidOperationException">成员不是字符串</exception>
    public static ComplexTypePropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>
    /// 按框架的精度常量声明 decimal 的精度
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
