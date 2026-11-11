using System.Linq.Expressions;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 框架的模型约定
/// </summary>
/// <remarks>
/// 在模块配置之后统一加上：审计列、软删除、并发版本号、删除规则、值对象列名、编码列的检查约束
/// </remarks>
internal static class ModelConventions
{
    private static readonly string _notDeleted = $"\"{SoftDeleteColumns.IsDeleted}\" = false";

    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => EntityKinds.IsBusinessEntity(e.ClrType)).ToList())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            // ID 由程序生成；不关掉的话，EF 会把带着 ID 的新子实体当成已有的行去更新
            entity.Property(nameof(Entity<>.Id)).ValueGeneratedNever();
            AddAuditColumns(entity);

            if (EntityKinds.IsAggregateRoot(entityType.ClrType))
            {
                // 领域事件和状态流转只在内存里传递，不映射到表
                entity.Ignore(nameof(IAggregateRoot.DomainEvents));
                entity.Ignore(nameof(IAggregateRoot.StateTransitions));
                entity.Property(nameof(IAggregateRoot.ConcurrencyVersion))
                    .IsConcurrencyToken()
                    .HasComment("并发版本号");

                // 列表默认按创建时间排序，所以要有索引
                entity.HasIndex(AuditColumns.CreatedAt);
            }

            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                AddSoftDelete(entity, entityType);
            }

            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
            {
                ApplyDeleteRule(foreignKey);
            }

            foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
            {
                NameValueObjectColumns(complexProperty, complexProperty.Name);
            }

            AddCodeChecks(entityType);
            AddColumnComments(entityType);
        }
    }

    // 列注释默认取属性的 XML 注释，配置里写了 HasComment 的不覆盖
    private static void AddColumnComments(IMutableEntityType entityType)
    {
        foreach (var property in entityType.GetDeclaredProperties().Where(p => p.GetComment() is null))
        {
            property.SetComment(property.IsPrimaryKey() && string.Equals(property.Name, nameof(Entity<>.Id), StringComparison.Ordinal)
                ? "ID"
                : GeneratedComments.Of(property.PropertyInfo) ?? ParentKeyComment(property));
        }

        foreach (var complexProperty in entityType.GetDeclaredComplexProperties().Where(c => !c.IsCollection && !c.ComplexType.IsMappedToJson()))
        {
            CommentValueObjectColumns(complexProperty, GeneratedComments.Of(complexProperty.PropertyInfo));
        }
    }

    // 值对象的列注释是“属性注释：成员注释”，如“预算：金额”
    private static void CommentValueObjectColumns(IMutableComplexProperty complexProperty, string? owner)
    {
        foreach (var property in complexProperty.ComplexType.GetDeclaredProperties().Where(p => p.GetComment() is null))
        {
            var member = GeneratedComments.Of(property.PropertyInfo);
            property.SetComment(owner is null || member is null ? owner ?? member : $"{owner}：{member}");
        }

        foreach (var nested in complexProperty.ComplexType.GetDeclaredComplexProperties().Where(c => !c.IsCollection && !c.ComplexType.IsMappedToJson()))
        {
            var member = GeneratedComments.Of(nested.PropertyInfo);
            CommentValueObjectColumns(nested, owner is null || member is null ? owner ?? member : $"{owner}：{member}");
        }
    }

    // 子实体指向父实体的外键
    private static string? ParentKeyComment(IMutableProperty property) =>
        property.IsShadowProperty() && property.GetContainingForeignKeys().FirstOrDefault(fk => fk.PrincipalToDependent is not null) is { } foreignKey
            ? $"{foreignKey.PrincipalEntityType.ClrType.Name} ID"
            : null;

    // 值对象的列名是属性名加成员名，如 BudgetAmount。存成 jsonb 的值对象只有一列，不用处理
    private static void NameValueObjectColumns(IMutableComplexProperty complexProperty, string prefix)
    {
        if (complexProperty.IsCollection || complexProperty.ComplexType.IsMappedToJson())
        {
            return;
        }

        foreach (var property in complexProperty.ComplexType.GetDeclaredProperties())
        {
            property.SetColumnName(prefix + property.Name);
        }

        foreach (var nested in complexProperty.ComplexType.GetDeclaredComplexProperties())
        {
            NameValueObjectColumns(nested, prefix + nested.Name);
        }
    }

    // 枚举列只能存已定义的编码，新增取值时迁移里会出现约束变更
    private static void AddCodeChecks(IMutableEntityType entityType)
    {
        var table = entityType.GetTableName();
        foreach (var property in ColumnProperties(entityType))
        {
            var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (!type.IsEnum && !ValueTypeConventions.IsEnumeration(type))
            {
                continue;
            }

            var column = property.GetColumnName();
            var codes = string.Join(", ", ValueTypeConventions.CodesOf(type).Select(code => $"'{code.Replace("'", "''", StringComparison.Ordinal)}'"));
            entityType.AddCheckConstraint($"CK_{table}_{column}", $"\"{column}\" IN ({codes})");
        }
    }

    private static IEnumerable<IMutableProperty> ColumnProperties(IMutableTypeBase type) =>
        type.GetDeclaredProperties().Concat(type.GetDeclaredComplexProperties()
            .Where(c => !c.IsCollection && !c.ComplexType.IsMappedToJson())
            .SelectMany(c => ColumnProperties(c.ComplexType)));

    private static void ApplyDeleteRule(IMutableForeignKey foreignKey)
    {
        if (foreignKey.PrincipalToDependent is not null && !EntityKinds.IsAggregateRoot(foreignKey.DeclaringEntityType.ClrType))
        {
            // 外键可空的话，子实体离开集合时 EF 只会把外键置空，行还留在表里
            foreignKey.IsRequired = true;
            foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
        }
        else if (EntityKinds.IsAggregateRoot(foreignKey.PrincipalEntityType.ClrType))
        {
            // 还被别的聚合引用时，不允许物理删除
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static void AddAuditColumns(EntityTypeBuilder entity)
    {
        entity.Property<DateTimeOffset>(AuditColumns.CreatedAt).HasComment("创建时间");
        entity.Property<UserId>(AuditColumns.CreatedBy).HasComment("创建人");
        entity.Property<DateTimeOffset?>(AuditColumns.UpdatedAt).HasComment("修改时间");
        entity.Property<UserId?>(AuditColumns.UpdatedBy).HasComment("修改人");
    }

    private static void AddSoftDelete(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        entity.Property<bool>(SoftDeleteColumns.IsDeleted).HasDefaultValue(false).HasComment("是否已删除");
        entity.Property<DateTimeOffset?>(SoftDeleteColumns.DeletedAt).HasComment("删除时间");
        entity.Property<UserId?>(SoftDeleteColumns.DeletedBy).HasComment("删除人");
        entity.HasQueryFilter(QueryFilters.SoftDelete, NotDeleted(entityType.ClrType));

        // 唯一约束只管没删除的行，删除后才能新建同编号的数据
        foreach (var index in entityType.GetIndexes().Where(i => i.IsUnique))
        {
            index.SetFilter(index.GetFilter() is { } existing ? $"({existing}) AND {_notDeleted}" : _notDeleted);
        }
    }

    private static LambdaExpression NotDeleted(Type clrType)
    {
        var entity = Expression.Parameter(clrType, "e");
        var isDeleted = Expression.Call(
            typeof(EF), nameof(EF.Property), [typeof(bool)], entity, Expression.Constant(SoftDeleteColumns.IsDeleted));
        return Expression.Lambda(Expression.Not(isDeleted), entity);
    }
}
