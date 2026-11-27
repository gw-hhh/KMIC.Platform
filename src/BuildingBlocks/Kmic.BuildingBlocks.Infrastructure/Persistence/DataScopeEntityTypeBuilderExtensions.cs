using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>数据权限配置扩展（实现 IHasOwner 或 IHasOrgUnit 的聚合根须显式调用 HasDataScope 或 HasNoDataScope）</summary>
public static class DataScopeEntityTypeBuilderExtensions
{
    /// <summary>启用按数据权限范围过滤</summary>
    public static void HasDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string scopeObject)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        builder.Metadata.SetAnnotation(ModelAnnotations.DataScope, scopeObject);
    }

    /// <summary>显式禁用数据权限过滤并说明原因</summary>
    public static void HasNoDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string reason)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        builder.Metadata.SetAnnotation(ModelAnnotations.NoDataScope, reason);
    }
}
