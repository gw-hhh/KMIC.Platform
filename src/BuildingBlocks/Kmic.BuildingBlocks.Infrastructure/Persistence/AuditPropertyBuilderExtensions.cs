using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Compliance.Classification;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>属性审计脱敏与数据分类扩展（默认完整记录修改前后值，敏感字段须显式标记）</summary>
public static class AuditPropertyBuilderExtensions
{
    /// <summary>审计时部分遮盖（如手机号 138****1234）</summary>
    public static PropertyBuilder<TProperty> AuditMasked<TProperty>(this PropertyBuilder<TProperty> builder, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Masked(mask));
    }

    /// <summary>审计时仅记录“已修改”而不记录具体值，适用于密码哈希、令牌等</summary>
    public static PropertyBuilder<TProperty> AuditHidden<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Hidden);
    }

    /// <summary>从审计记录中排除（适用于缓存类的派生字段）</summary>
    public static PropertyBuilder<TProperty> ExcludeFromAudit<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Excluded);
    }

    /// <summary>审计时部分遮盖（如手机号 138****1234）</summary>
    public static ComplexTypePropertyBuilder<TProperty> AuditMasked<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Masked(mask));
    }

    /// <summary>审计时仅记录“已修改”而不记录具体值</summary>
    public static ComplexTypePropertyBuilder<TProperty> AuditHidden<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Hidden);
    }

    /// <summary>从审计记录中排除</summary>
    public static ComplexTypePropertyBuilder<TProperty> ExcludeFromAudit<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasAnnotation(AuditAnnotations.Name, AuditAnnotations.Excluded);
    }

    /// <summary>声明字段的数据分类，使审计与日志按统一规则脱敏</summary>
    public static PropertyBuilder<TProperty> Classified<TProperty>(this PropertyBuilder<TProperty> builder, DataClassification classification)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AuditAnnotations.For(classification) is { } mode ? builder.HasAnnotation(AuditAnnotations.Name, mode) : builder;
    }

    /// <summary>声明值对象成员的数据分类，使审计与日志按统一规则脱敏</summary>
    public static ComplexTypePropertyBuilder<TProperty> Classified<TProperty>(this ComplexTypePropertyBuilder<TProperty> builder, DataClassification classification)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return AuditAnnotations.For(classification) is { } mode ? builder.HasAnnotation(AuditAnnotations.Name, mode) : builder;
    }
}
