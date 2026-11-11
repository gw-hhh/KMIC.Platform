using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 保存前按固定顺序执行的规则（05-03 第 7.4 节的 1～3）：软删除 → 审计字段 → 聚合版本
/// </summary>
/// <remarks>
/// 软删除必须最先：删除被改写成修改后，才会被审计和版本计为一次修改
/// </remarks>
internal static class SaveRules
{
    public static void Apply(DbContext context, IExecutionContext execution, DateTimeOffset now)
    {
        var actor = Actor(execution.User);
        var changed = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && EntityKinds.IsBusinessEntity(e.Metadata.ClrType))
            .ToList();

        foreach (var entry in changed.Where(e => e is { State: EntityState.Deleted, Entity: ISoftDeletable }))
        {
            entry.State = EntityState.Modified;
            entry.Property(SoftDeleteColumns.IsDeleted).CurrentValue = true;
            entry.Property(SoftDeleteColumns.DeletedAt).CurrentValue = now;
            entry.Property(SoftDeleteColumns.DeletedBy).CurrentValue = actor;
        }

        foreach (var entry in changed)
        {
            StampAudit(entry, actor, now);
        }

        foreach (var root in AffectedRoots(context, changed))
        {
            // 只改了子实体时，聚合根本身也算被修改：版本加一，修改人和时间一并更新
            if (root.State == EntityState.Unchanged)
            {
                root.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                root.Property(AuditColumns.UpdatedBy).CurrentValue = actor;
            }

            var version = root.Property(nameof(IAggregateRoot.ConcurrencyVersion));
            version.CurrentValue = (long)version.OriginalValue! + 1;
        }
    }

    // 模拟身份记实际操作人（05-03 D4）；系统身份与匿名请求（登录等）记系统账号
    private static UserId Actor(ICurrentUser user) => user.ActualUserId ?? user.UserId ?? UserId.System;

    private static void StampAudit(EntityEntry entry, UserId actor, DateTimeOffset now)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Property(AuditColumns.CreatedAt).CurrentValue = now;
            entry.Property(AuditColumns.CreatedBy).CurrentValue = actor;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Property(AuditColumns.UpdatedAt).CurrentValue = now;
            entry.Property(AuditColumns.UpdatedBy).CurrentValue = actor;
        }
    }

    // 新增的聚合根从 0 开始，删除的聚合根靠 DELETE 条件中的原版本做检查，两者都不递增
    private static List<EntityEntry> AffectedRoots(DbContext context, List<EntityEntry> changed)
    {
        var roots = new List<EntityEntry>();
        foreach (var entry in changed)
        {
            var root = RootOf(context, entry);
            if (root.State is EntityState.Unchanged or EntityState.Modified && !roots.Exists(r => ReferenceEquals(r.Entity, root.Entity)))
            {
                roots.Add(root);
            }
        }

        return roots;
    }

    private static EntityEntry RootOf(DbContext context, EntityEntry entry)
    {
        var current = entry;
        while (current.Entity is not IAggregateRoot)
        {
            current = ParentOf(context, current)
                ?? throw new InvalidOperationException(
                    $"{current.Metadata.DisplayName()} 被修改，但它所属的聚合根没有被跟踪：子实体只能经仓储加载完整聚合后修改。");
        }

        return current;
    }

    // 父子关系取自 EF 模型中父实体指向子实体的导航，子实体不需要额外声明
    private static EntityEntry? ParentOf(DbContext context, EntityEntry child)
    {
        var foreignKey = child.Metadata.GetForeignKeys().FirstOrDefault(fk => fk.PrincipalToDependent is not null);
        if (foreignKey is null)
        {
            return null;
        }

        var parentKey = foreignKey.Properties.Select(p => ParentKeyValue(child.Property(p.Name))).ToArray();
        return context.ChangeTracker.Entries().FirstOrDefault(e =>
            foreignKey.PrincipalEntityType.IsAssignableFrom(e.Metadata)
            && foreignKey.PrincipalKey.Properties.Select(p => e.Property(p.Name).CurrentValue).SequenceEqual(parentKey));
    }

    // 从集合中移除的子实体在保存前仍是 Modified，外键已被置空，要按原值找到它原来的聚合根
    private static object? ParentKeyValue(PropertyEntry foreignKey) =>
        foreignKey.EntityEntry.State == EntityState.Deleted || foreignKey.CurrentValue is null ? foreignKey.OriginalValue : foreignKey.CurrentValue;
}
