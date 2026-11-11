using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在实体配置中声明聚合的数据范围
/// </summary>
/// <remarks>
/// 实现 IHasOwner / IHasOrgUnit 的聚合根必须二选一：HasDataScope 或 HasNoDataScope，否则模型校验失败
/// </remarks>
public static class DataScopeEntityTypeBuilderExtensions
{
    /// <summary>
    /// 按范围对象过滤：模块配置完成后，框架为该实体注册 DataScope 命名过滤器
    /// </summary>
    /// <param name="builder">聚合根的实体配置</param>
    /// <param name="scopeObject">本模块声明的范围对象编码，如 SampleScopes.Sample</param>
    public static void HasDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string scopeObject)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        builder.Metadata.SetAnnotation(ModelAnnotations.DataScope, scopeObject);
    }

    /// <summary>
    /// 有所属人或所属组织，但不按数据范围过滤
    /// </summary>
    /// <param name="builder">聚合根的实体配置</param>
    /// <param name="reason">不过滤的原因，供安全审查，如“产品目录对所有人可见”</param>
    public static void HasNoDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string reason)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        builder.Metadata.SetAnnotation(ModelAnnotations.NoDataScope, reason);
    }
}
