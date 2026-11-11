using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 保存前的规则
/// </summary>
/// <remarks>
/// 依次处理软删除、审计列、版本号。软删除要最先：删除改成修改后，审计和版本号才会按一次修改处理
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
            // 只改了子实体时，聚合根也算修改：版本号加一，修改人和修改时间一起更新
            if (root.State == EntityState.Unchanged)
            {
                root.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                root.Property(AuditColumns.UpdatedBy).CurrentValue = actor;
            }

            var version = root.Property(nameof(IAggregateRoot.ConcurrencyVersion));
            version.CurrentValue = (long)version.OriginalValue! + 1;
        }
    }

    // 模拟登录时记实际操作人，系统身份和匿名请求（如登录）记系统用户
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

    // 新增和删除的聚合根不加版本号：新增的从 0 开始，删除靠 DELETE 条件里的原版本号检查冲突
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
                    $"{current.Metadata.DisplayName()} 被修改了，但它的聚合根没有被跟踪。子实体要通过仓储读取整个聚合后再修改。");
        }

        return current;
    }

    // 父子关系取自模型里父实体指向子实体的导航，子实体不用另外声明
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

    // 离开集合的子实体在保存前还是 Modified，外键已经被清空，要用原值找到它的聚合根
    private static object? ParentKeyValue(PropertyEntry foreignKey) =>
        foreignKey.EntityEntry.State == EntityState.Deleted || foreignKey.CurrentValue is null ? foreignKey.OriginalValue : foreignKey.CurrentValue;
}
