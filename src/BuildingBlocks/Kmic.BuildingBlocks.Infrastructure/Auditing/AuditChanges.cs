using System.Text.Json;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

// 须在保存前调用：从变更跟踪器采集本次数据变更
internal static class AuditChanges
{
    private static readonly HashSet<string> _skipped =
    [
        AuditColumns.CreatedAt, AuditColumns.CreatedBy, AuditColumns.UpdatedAt, AuditColumns.UpdatedBy,
        nameof(IAggregateRoot.ConcurrencyVersion), SoftDeleteColumns.IsDeleted, SoftDeleteColumns.DeletedAt, SoftDeleteColumns.DeletedBy,
    ];

    public static (IReadOnlyList<EntityChange> Changes, IReadOnlyList<AuditTarget> Targets) Collect(DbContext context, int maxValueLength)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 移出集合的子实体需触发变更检测后才会标记为删除
        context.ChangeTracker.DetectChanges();
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && EntityKinds.IsBusinessEntity(e.Metadata.ClrType))
            .ToList();

        var changes = new List<EntityChange>();
        var targets = new List<AuditTarget>();
        foreach (var entry in entries)
        {
            var kind = KindOf(entry);
            var fields = kind is ChangeKind.Added or ChangeKind.Modified ? Fields(entry, kind == ChangeKind.Added, maxValueLength) : [];
            if (kind == ChangeKind.Modified && fields.Count == 0)
            {
                continue;
            }

            changes.Add(new EntityChange(entry.Metadata.ClrType.Name, IdOf(entry), kind, fields));
            var root = SaveRules.RootOf(context, entry);
            var target = new AuditTarget(root.Metadata.ClrType.Name, IdOf(root));
            if (!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        return (changes, targets);
    }

    private static Guid IdOf(EntityEntry entry) => ((IStronglyTypedId)entry.Property(nameof(Entity<>.Id)).CurrentValue!).Value;

    private static ChangeKind KindOf(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
        {
            return ChangeKind.Added;
        }

        var softDeletable = entry.Entity is ISoftDeletable;
        if (entry.State == EntityState.Deleted)
        {
            return softDeletable ? ChangeKind.SoftDeleted : ChangeKind.Deleted;
        }

        if (softDeletable && entry.Property(SoftDeleteColumns.IsDeleted) is { IsModified: true } flag && (bool)flag.CurrentValue! != (bool)flag.OriginalValue!)
        {
            return (bool)flag.CurrentValue! ? ChangeKind.SoftDeleted : ChangeKind.Restored;
        }

        return ChangeKind.Modified;
    }

    private static List<FieldChange> Fields(EntityEntry entry, bool added, int maxValueLength)
    {
        var fields = new List<FieldChange>();
        foreach (var property in entry.Properties.Where(p => !IsSkipped(p.Metadata)))
        {
            Add(fields, property.Metadata.Name, property, added, maxValueLength);
        }

        foreach (var complex in entry.ComplexProperties)
        {
            AddComplex(fields, complex.Metadata.Name, complex, added, maxValueLength);
        }

        foreach (var collection in entry.ComplexCollections.Where(c => added || c.IsModified))
        {
            var before = added ? null : entry.OriginalValues[collection.Metadata];
            var field = AddedOrChanged(PropertyNameOf(collection.Metadata.Name), Json(before), Json(collection.CurrentValue), AuditAnnotations.ModeOf(collection.Metadata), maxValueLength);
            if (field is not null && !(added && field.New is null))
            {
                fields.Add(field);
            }
        }

        return fields;
    }

    private static void AddComplex(List<FieldChange> fields, string path, ComplexPropertyEntry complex, bool added, int maxValueLength)
    {
        foreach (var property in complex.Properties.Where(p => !IsSkipped(p.Metadata)))
        {
            Add(fields, $"{path}.{property.Metadata.Name}", property, added, maxValueLength);
        }

        foreach (var nested in complex.ComplexProperties)
        {
            AddComplex(fields, $"{path}.{nested.Metadata.Name}", nested, added, maxValueLength);
        }
    }

    private static void Add(List<FieldChange> fields, string name, PropertyEntry property, bool added, int maxValueLength)
    {
        if (!added && !property.IsModified)
        {
            return;
        }

        var converter = property.Metadata.GetValueConverter();
        var old = added ? null : Format(property.OriginalValue, converter);
        var current = Format(property.CurrentValue, converter);

        // 新增时忽略空值字段；修改时忽略原值与现值相同的字段（如改后又改回原值）
        if ((added && current is null) || string.Equals(old, current, StringComparison.Ordinal))
        {
            return;
        }

        if (AddedOrChanged(name, old, current, AuditAnnotations.ModeOf(property.Metadata), maxValueLength) is { } field)
        {
            fields.Add(field);
        }
    }

    private static FieldChange? AddedOrChanged(string name, string? old, string? current, string? mode, int maxValueLength)
    {
        if (string.Equals(mode, AuditAnnotations.Excluded, StringComparison.Ordinal))
        {
            return null;
        }

        if (string.Equals(mode, AuditAnnotations.Hidden, StringComparison.Ordinal))
        {
            return new FieldChange(name, null, null, Masked: true);
        }

        if (AuditAnnotations.TryReadMask(mode, out var mask))
        {
            return new FieldChange(
                name,
                old is null ? null : AuditValues.Mask(old, mask),
                current is null ? null : AuditValues.Mask(current, mask),
                Masked: true);
        }

        return new FieldChange(name, AuditValues.Truncate(old, maxValueLength), AuditValues.Truncate(current, maxValueLength), Masked: false);
    }

    private static string? Format(object? value, ValueConverter? converter) =>
        AuditValues.Format(value is null || converter is null ? value : converter.ConvertToProvider(value));


    // 值对象集合映射私有字段 _watchers，对外暴露为属性 Watchers
    private static string PropertyNameOf(string name) =>
        name.Length > 1 && name[0] == '_' ? char.ToUpperInvariant(name[1]) + name[2..] : name;
    private static string? Json(object? value) => value is null ? null : JsonSerializer.Serialize(value, value.GetType(), KmicJsonOptions.Stored);

    private static bool IsSkipped(IProperty property) =>
        property.IsPrimaryKey() || property.IsForeignKey() || _skipped.Contains(property.Name) || string.Equals(AuditAnnotations.ModeOf(property), AuditAnnotations.Excluded, StringComparison.Ordinal);
}
