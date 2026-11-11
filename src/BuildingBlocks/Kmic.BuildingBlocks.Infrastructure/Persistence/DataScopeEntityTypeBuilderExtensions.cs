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
    /// <param name="builder">聚合根的配置</param>
    /// <param name="scopeObject">范围对象编码，如 SampleScopes.Sample</param>
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
    /// <param name="builder">聚合根的配置</param>
    /// <param name="reason">原因，如“产品目录对所有人可见”</param>
    public static void HasNoDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string reason)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        builder.Metadata.SetAnnotation(ModelAnnotations.NoDataScope, reason);
    }
}
