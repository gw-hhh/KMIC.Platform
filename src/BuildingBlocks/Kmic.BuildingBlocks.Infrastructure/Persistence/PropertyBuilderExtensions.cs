using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 实体配置中使用的框架映射扩展
/// </summary>
/// <remarks>
/// 写成泛型是为了同时接受 string 与 string? 属性（可空注解不构成不同的类型），非字符串属性在构建模型时报错
/// </remarks>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// 长文本，映射为 text；字符串属性要么声明最大长度，要么显式用它
    /// </summary>
    /// <exception cref="InvalidOperationException">属性不是字符串</exception>
    public static PropertyBuilder<TProperty> IsUnboundedText<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsUnboundedText));
        return builder.HasColumnType(ColumnTypes.UnboundedText);
    }

    /// <summary>
    /// 不区分大小写：等值比较与唯一约束都忽略大小写，适合账号、邮箱、编码
    /// </summary>
    /// <remarks>
    /// 非确定性排序规则不支持 ILIKE，模糊匹配统一用 KmicFunctions
    /// </remarks>
    /// <exception cref="InvalidOperationException">属性不是字符串</exception>
    public static PropertyBuilder<TProperty> IsCaseInsensitive<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureString(builder, nameof(IsCaseInsensitive));
        return builder.UseCollation(Collations.CaseInsensitive);
    }

    /// <summary>
    /// 按框架的精度常量声明 decimal 的精度，如 Precisions.Amount
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
