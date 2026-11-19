using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 数据权限声明
/// </summary>
/// <remarks>
/// 实现了 IHasOwner 或 IHasOrgUnit 的聚合根，必须调用 HasDataScope 或 HasNoDataScope 之一
/// </remarks>
public static class DataScopeEntityTypeBuilderExtensions
{
    /// <summary>
    /// 按数据权限过滤
    /// </summary>
    public static void HasDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string scopeObject)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        builder.Metadata.SetAnnotation(ModelAnnotations.DataScope, scopeObject);
    }

    /// <summary>
    /// 不按数据权限过滤
    /// </summary>
    public static void HasNoDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string reason)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        builder.Metadata.SetAnnotation(ModelAnnotations.NoDataScope, reason);
    }
}
