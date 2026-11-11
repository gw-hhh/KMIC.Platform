using System.Linq.Expressions;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在模块配置之后统一加上的框架约定：审计列、软删除、并发版本、关系的删除规则
/// </summary>
internal static class ModelConventions
{
    private static readonly string _notDeleted = $"\"{SoftDeleteColumns.IsDeleted}\" = false";

    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => EntityKinds.IsBusinessEntity(e.ClrType)).ToList())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            // ID 由程序生成（ADR-0009）；否则 EF 会把带着 ID 的新子实体当成已存在的行去更新
            entity.Property(nameof(Entity<>.Id)).ValueGeneratedNever();
            AddAuditColumns(entity);

            if (EntityKinds.IsAggregateRoot(entityType.ClrType))
            {
                // 领域事件与状态流转只在内存中传递，由事务行为与拦截器取走，不映射到表
                entity.Ignore(nameof(IAggregateRoot.DomainEvents));
                entity.Ignore(nameof(IAggregateRoot.StateTransitions));
                entity.Property(nameof(IAggregateRoot.ConcurrencyVersion))
                    .IsConcurrencyToken()
                    .HasComment("并发版本：聚合内任何实体新增、修改、删除都加一");

                // 列表默认按创建时间排序（ADR-0009：不按 ID 排序）
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
        }
    }

    // 父实体 → 子实体：外键必填并级联删除，从集合中移除的子实体随保存删除，而不是留下外键为空的行；
    // 聚合之间只存 ID：外键 Restrict，仍被引用的聚合不能物理删除
    private static void ApplyDeleteRule(IMutableForeignKey foreignKey)
    {
        if (foreignKey.PrincipalToDependent is not null && !EntityKinds.IsAggregateRoot(foreignKey.DeclaringEntityType.ClrType))
        {
            foreignKey.IsRequired = true;
            foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
        }
        else if (EntityKinds.IsAggregateRoot(foreignKey.PrincipalEntityType.ClrType))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static void AddAuditColumns(EntityTypeBuilder entity)
    {
        entity.Property<DateTimeOffset>(AuditColumns.CreatedAt).HasComment("创建时刻");
        entity.Property<UserId>(AuditColumns.CreatedBy).HasComment("创建人；模拟身份时为实际操作人，系统身份为系统账号");
        entity.Property<DateTimeOffset?>(AuditColumns.UpdatedAt).HasComment("最后修改时刻；新增后未修改过为空");
        entity.Property<UserId?>(AuditColumns.UpdatedBy).HasComment("最后修改人");
    }

    private static void AddSoftDelete(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        entity.Property<bool>(SoftDeleteColumns.IsDeleted).HasDefaultValue(false).HasComment("是否已删除（软删除）");
        entity.Property<DateTimeOffset?>(SoftDeleteColumns.DeletedAt).HasComment("删除时刻");
        entity.Property<UserId?>(SoftDeleteColumns.DeletedBy).HasComment("删除人");
        entity.HasQueryFilter(QueryFilters.SoftDelete, NotDeleted(entityType.ClrType));

        // 删除后要能新建同编号的数据：唯一约束只约束未删除的行
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
