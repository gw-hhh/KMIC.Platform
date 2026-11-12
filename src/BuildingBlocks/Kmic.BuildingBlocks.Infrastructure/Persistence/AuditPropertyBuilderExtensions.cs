using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 审计记录里怎样处理这个字段
/// </summary>
/// <remarks>
/// 默认完整记录修改前后的值。敏感字段在实体配置里标出来，审计和日志共用这些标注
/// </remarks>
public static class AuditPropertyBuilderExtensions
{
    /// <summary>
    /// 部分遮盖，如手机号 138****1234
    /// </summary>
    public static PropertyBuilder<TProperty> AuditMasked<TProperty>(this PropertyBuilder<TProperty> builder, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Masked(mask));
    }

    /// <summary>
    /// 只记录“已修改”，不记录值，用于密码哈希、令牌这类字段
    /// </summary>
    public static PropertyBuilder<TProperty> AuditHidden<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Hidden);
    }

    /// <summary>
    /// 不进审计记录，用于缓存类的派生字段
    /// </summary>
    public static PropertyBuilder<TProperty> ExcludeFromAudit<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Excluded);
    }

    /// <summary>
    /// 部分遮盖，如手机号 138****1234
    /// </summary>
    public static ComplexTypePropertyBuilder<TProperty> AuditMasked<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Masked(mask));
    }

    /// <summary>
    /// 只记录“已修改”，不记录值
    /// </summary>
    public static ComplexTypePropertyBuilder<TProperty> AuditHidden<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Hidden);
    }

    /// <summary>
    /// 不进审计记录
    /// </summary>
    public static ComplexTypePropertyBuilder<TProperty> ExcludeFromAudit<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Excluded);
    }

    /// <summary>
    /// 声明字段的数据分类，审计记录和日志按同一套规则处理
    /// </summary>
    /// <remarks>
    /// Secret 在审计里只记“已修改”，Personal 部分遮盖，Internal 完整记录。等价于对应的 AuditHidden、AuditMasked
    /// </remarks>
    public static PropertyBuilder<TProperty> Classified<TProperty>(this PropertyBuilder<TProperty> builder, DataClassification classification)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AuditAnnotations.For(classification) is { } mode ? builder.HasAnnotation(AuditAnnotations.Name, mode) : builder;
    }

    /// <summary>
    /// 声明值对象成员的数据分类，审计记录和日志按同一套规则处理
    /// </summary>
    public static ComplexTypePropertyBuilder<TProperty> Classified<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, DataClassification classification)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AuditAnnotations.For(classification) is { } mode ? builder.HasAnnotation(AuditAnnotations.Name, mode) : builder;
    }
}
