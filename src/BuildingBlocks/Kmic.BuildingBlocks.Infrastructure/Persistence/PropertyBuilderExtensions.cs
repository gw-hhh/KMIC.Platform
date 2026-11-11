using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 实体配置中使用的框架映射扩展
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// 长文本，映射为 text；字符串属性要么声明最大长度，要么显式用它
    /// </summary>
    /// <remarks>
    /// 写成泛型是为了同时接受 string 与 string? 属性（可空注解不构成不同的类型）
    /// </remarks>
    /// <exception cref="InvalidOperationException">属性不是字符串</exception>
    public static PropertyBuilder<TProperty> IsUnboundedText<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return typeof(TProperty) == typeof(string)
            ? builder.HasColumnType(ColumnTypes.UnboundedText)
            : throw new InvalidOperationException($"IsUnboundedText 只能用于字符串属性，{builder.Metadata.Name} 的类型是 {typeof(TProperty).Name}。");
    }
}
