using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模型校验
/// </summary>
/// <remarks>
/// 有任何问题都拒绝启动，并列出全部问题。架构测试跑的是同一套校验
/// </remarks>
internal static class ModelValidator
{
    private static readonly string[] _secretWords = ["Password", "Token", "Secret", "Hash"];

    public static void Validate(IReadOnlyModel model, string module, DataScopeObjectCatalog scopeObjects)
    {
        var problems = new List<string>();
        foreach (var entityType in model.GetEntityTypes())
        {
            ValidateEntity(entityType, module, problems);
            ValidateDataScope(entityType, module, scopeObjects, problems);
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException($"{module} 模块的数据模型", problems);
        }
    }

    private static void ValidateEntity(IReadOnlyEntityType entityType, string module, List<string> problems)
    {
        var name = entityType.DisplayName();
        if (entityType.GetTableName() is not null && !string.Equals(entityType.GetSchema(), module, StringComparison.Ordinal))
        {
            problems.Add($"{name}：映射到 schema {entityType.GetSchema()}，只能映射到本模块的 {module}。");
        }

        if (EntityKinds.IsBusinessEntity(entityType.ClrType) && !string.Equals(entityType.GetTableName(), entityType.ClrType.Name, StringComparison.Ordinal))
        {
            problems.Add($"{name}：表名 {entityType.GetTableName()} 必须与实体类名 {entityType.ClrType.Name} 相同。");
        }

        foreach (var (path, property, inJson) in AllProperties(entityType, name, inJson: false))
        {
            ValidateProperty(path, property, inJson, problems);
        }

        if (EntityKinds.IsBusinessEntity(entityType.ClrType))
        {
            ValidateComments(entityType, name, problems);
            ValidateAuditMarks(entityType, name, problems);
            foreach (var index in entityType.GetDeclaredIndexes().Where(i => i.IsUnique && i.FindAnnotation(ModelAnnotations.UniqueError) is null))
            {
                problems.Add($"{name}：唯一索引（{string.Join(", ", index.Properties.Select(p => p.Name))}）没有指定 MapsToError。");
            }
        }

        foreach (var navigation in entityType.GetDeclaredNavigations())
        {
            // 先认出子实体指回父实体的导航：父实体可能就是聚合根，不能当成两个聚合根之间的导航
            if (navigation.IsOnDependent && navigation.Inverse is not null)
            {
                problems.Add($"{name}.{navigation.Name}：导航只能从父实体指向子实体。");
            }
            else if (EntityKinds.IsAggregateRoot(navigation.TargetEntityType.ClrType))
            {
                problems.Add($"{name}.{navigation.Name}：聚合之间只存 ID，不能建导航属性。");
            }
            else if (navigation.IsOnDependent)
            {
                problems.Add($"{name}.{navigation.Name}：导航只能从父实体指向子实体。");
            }
        }

        if (EntityKinds.IsAggregateRoot(entityType.ClrType)
            && entityType.FindProperty(nameof(IAggregateRoot.ConcurrencyVersion)) is not { IsConcurrencyToken: true })
        {
            problems.Add($"{name}：聚合根的 ConcurrencyVersion 必须是并发令牌。");
        }

        if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) && !EntityKinds.IsAggregateRoot(entityType.ClrType))
        {
            problems.Add($"{name}：只有聚合根能实现 ISoftDeletable。");
        }
    }

    // 列注释是数据字典的来源，所以必须有。存成 jsonb 的值对象集合只有一列，成员没有列注释
    private static void ValidateComments(IReadOnlyEntityType entityType, string name, List<string> problems)
    {
        if (entityType.GetComment() is null)
        {
            problems.Add($"{name}：表缺少注释，在实体配置中写 ToTable(t => t.HasComment(\"…\"))：中文名称、所属模块、一行代表什么、生命周期。");
        }

        foreach (var (path, property, inJson) in AllProperties(entityType, name, inJson: false))
        {
            if (!inJson && property.GetComment() is null)
            {
                problems.Add($"{path}：列缺少注释，给属性写 XML 注释（summary），说法需要不同时在配置中写 HasComment。");
            }
        }
    }


    // 名字里有这些词的字段多半是密钥类数据，审计记录里不能出现它们的值，要明确标出来
    private static void ValidateAuditMarks(IReadOnlyEntityType entityType, string name, List<string> problems)
    {
        foreach (var (path, property, _) in AllProperties(entityType, name, inJson: false))
        {
            if (_secretWords.Any(w => property.Name.Contains(w, StringComparison.Ordinal))
                && AuditAnnotations.ModeOf(property) is not (AuditAnnotations.Hidden or AuditAnnotations.Excluded))
            {
                problems.Add($"{path}：名字里有 Password、Token、Secret 或 Hash，要用 Classified(KmicData.Secret)（或 AuditHidden()）标出来，审计记录和日志里不能出现它的值。");
            }
        }
    }
    private static void ValidateProperty(string location, IReadOnlyProperty property, bool inJson, List<string> problems)
    {
        // 按存储类型判断：单值值对象存的是它包装的原始值
        var stored = property.GetValueConverter()?.ProviderClrType ?? property.ClrType;
        var type = Nullable.GetUnderlyingType(stored) ?? stored;
        if (type == typeof(float) || type == typeof(double))
        {
            problems.Add($"{location}：不能用 float 或 double，请用 decimal 并设置精度。");
        }

        if (type == typeof(DateTime))
        {
            problems.Add($"{location}：不能用 DateTime，请用 DateTimeOffset、DateOnly 或 TimeOnly。");
        }

        // jsonb 里的成员没有列类型，长度和精度由值对象自己保证
        if (inJson)
        {
            return;
        }

        if (type == typeof(decimal) && property.GetPrecision() is null)
        {
            problems.Add($"{location}：decimal 没有设置精度。");
        }

        if (type == typeof(string)
            && property.GetMaxLength() is null
            && property.GetColumnType() is not (ColumnTypes.UnboundedText or ColumnTypes.Json))
        {
            problems.Add($"{location}：字符串没有设置最大长度，长文本请用 IsUnboundedText()。");
        }
    }

    private static void ValidateDataScope(IReadOnlyEntityType entityType, string module, DataScopeObjectCatalog scopeObjects, List<string> problems)
    {
        var name = entityType.DisplayName();
        var declaredNone = entityType.FindAnnotation(ModelAnnotations.NoDataScope) is not null;
        var hasOwnership = typeof(IHasOwner).IsAssignableFrom(entityType.ClrType) || typeof(IHasOrgUnit).IsAssignableFrom(entityType.ClrType);
        var isRoot = EntityKinds.IsAggregateRoot(entityType.ClrType);

        if (entityType.FindAnnotation(ModelAnnotations.DataScope)?.Value is not string scopeObject)
        {
            if (isRoot && hasOwnership && !declaredNone)
            {
                problems.Add($"{name}：有所属人或所属组织，要调用 HasDataScope 或 HasNoDataScope。");
            }

            return;
        }

        if (declaredNone)
        {
            problems.Add($"{name}：不能同时声明 HasDataScope 与 HasNoDataScope。");
        }

        if (!isRoot || !hasOwnership)
        {
            // 子实体只能通过聚合根访问，没有自己的过滤器。没有归属的数据无法判断范围
            problems.Add($"{name}：只有实现 IHasOwner 或 IHasOrgUnit 的聚合根能声明 HasDataScope。");
        }

        if (!scopeObjects.Contains(scopeObject))
        {
            problems.Add($"{name}：范围对象 {scopeObject} 未定义。");
        }
        else if (!scopeObject.StartsWith(module + ".", StringComparison.Ordinal))
        {
            problems.Add($"{name}：范围对象 {scopeObject} 属于其他模块。");
        }
    }

    // 值对象的成员同样要满足规则，位置写成“实体.属性.成员”
    private static IEnumerable<(string Path, IReadOnlyProperty Property, bool InJson)> AllProperties(IReadOnlyTypeBase type, string path, bool inJson) =>
        type.GetDeclaredProperties().Select(p => ($"{path}.{p.Name}", p, inJson))
            .Concat(type.GetDeclaredComplexProperties().SelectMany(c =>
                AllProperties(c.ComplexType, $"{path}.{c.Name}", inJson || c.ComplexType.IsMappedToJson())));
}
