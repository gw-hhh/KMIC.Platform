using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 属性映射扩展
/// </summary>
/// <remarks>
/// 写成泛型才能同时接受 string 和 string? 属性，非字符串属性在建模时报错
/// </remarks>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// 不限长度的文本列
    /// </summary>
    /// <remarks>
    /// 字符串属性要么设置最大长度，要么显式调用它
    /// </remarks>
    /// <exception cref="InvalidOperationException">不是字符串</exception>
    public static PropertyBuilder<TProperty> IsUnboundedText<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>
    /// 不区分大小写
    /// </summary>
    /// <remarks>
    /// 比较和唯一约束都忽略大小写，适合账号、邮箱、编码。这类列不支持 ILIKE，模糊查询用 KmicFunctions
    /// </remarks>
    /// <exception cref="InvalidOperationException">不是字符串</exception>
    public static PropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>
    /// 设置小数精度，如 Precisions.Amount
    /// </summary>
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
