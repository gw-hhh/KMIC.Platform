using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal static class SaveRules
{
    public static void Apply(DbContext context, IExecutionContext execution, DateTimeOffset now)
    {
        var actor = Actor(execution.User);
        var changed = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && EntityKinds.IsBusinessEntity(e.Metadata.ClrType))
            .ToList();

        // 优先处理软删除：将删除状态转换为修改后，后续审计字段与并发版本号才能按修改操作统一更新
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
            // 仅修改子实体时亦视为聚合根变更：同步递增并发版本号并更新修改人与修改时间
            if (root.State == EntityState.Unchanged)
            {
                root.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                root.Property(AuditColumns.UpdatedBy).CurrentValue = actor;
            }

            var version = root.Property(nameof(IAggregateRoot.ConcurrencyVersion));
            version.CurrentValue = (long)version.OriginalValue! + 1;
        }
    }

    // 模拟身份操作时记录实际操作人，系统身份及匿名请求（如登录）则记录系统用户
    public static UserId Actor(ICurrentUser user) => user.ActualUserId ?? user.UserId ?? UserId.System;

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

    // 新增与物理删除的聚合根不递增版本号：新增记录初始版本为 0，物理删除通过 DELETE 语句中的原版本号条件检测并发冲突
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

    public static EntityEntry RootOf(DbContext context, EntityEntry entry)
    {
        var current = entry;
        while (current.Entity is not IAggregateRoot)
        {
            current = ParentOf(context, current)
                ?? throw new InvalidOperationException(
                    $"{current.Metadata.DisplayName()} 已被修改，但其聚合根没有被跟踪。子实体须通过仓储读取整个聚合后再行修改。");
        }

        return current;
    }

    // 依据模型中父实体指向子实体的导航属性识别聚合归属，子实体无需额外声明
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

    // 从集合移除的子实体在保存前可能处于 Modified 状态且外键已置空，需通过外键原始值追溯其所属聚合根
    private static object? ParentKeyValue(PropertyEntry foreignKey) =>
        foreignKey.EntityEntry.State == EntityState.Deleted || foreignKey.CurrentValue is null ? foreignKey.OriginalValue : foreignKey.CurrentValue;
}
