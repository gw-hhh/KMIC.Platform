using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 模型校验：构建模型时执行，任何一条不满足都拒绝启动，并列出全部问题
/// </summary>
/// <remarks>
/// 架构测试构建每个模块的模型，执行的是同一套校验
/// </remarks>
internal static class ModelValidator
{
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

        foreach (var property in AllProperties(entityType))
        {
            ValidateProperty(name, property, problems);
        }

        foreach (var navigation in entityType.GetDeclaredNavigations())
        {
            // 先认出“子实体指回父实体”：父实体可能就是聚合根，不能当成聚合之间的导航
            if (navigation.IsOnDependent && navigation.Inverse is not null)
            {
                problems.Add($"{name}.{navigation.Name}：导航只能从父实体指向子实体，子实体不保留指回父实体的导航。");
            }
            else if (EntityKinds.IsAggregateRoot(navigation.TargetEntityType.ClrType))
            {
                problems.Add($"{name}.{navigation.Name}：聚合之间只存 ID，不建导航属性。");
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

    private static void ValidateProperty(string entity, IReadOnlyProperty property, List<string> problems)
    {
        var location = $"{entity}.{property.Name}";
        var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
        if (type == typeof(decimal) && property.GetPrecision() is null)
        {
            problems.Add($"{location}：decimal 必须声明精度，用 HasPrecision。");
        }

        if (type == typeof(float) || type == typeof(double))
        {
            problems.Add($"{location}：不能用 float / double 存数据，金额、数量等用 decimal 并声明精度。");
        }

        if (type == typeof(DateTime))
        {
            problems.Add($"{location}：不能用 DateTime，时刻用 DateTimeOffset，日期用 DateOnly，时间用 TimeOnly。");
        }

        if (type == typeof(string)
            && property.GetMaxLength() is null
            && !string.Equals(property.GetColumnType(), ColumnTypes.UnboundedText, StringComparison.Ordinal))
        {
            problems.Add($"{location}：字符串必须声明最大长度（HasMaxLength），长文本显式写 IsUnboundedText()。");
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
                problems.Add($"{name}：有所属人或所属组织，必须声明 HasDataScope(范围对象)，不需要过滤时写 HasNoDataScope(原因)。");
            }

            return;
        }

        if (declaredNone)
        {
            problems.Add($"{name}：不能同时声明 HasDataScope 与 HasNoDataScope。");
        }

        if (!isRoot || !hasOwnership)
        {
            // 子实体没有自己的过滤器，只能经聚合根访问；没有归属的数据无从判断范围
            problems.Add($"{name}：只有实现 IHasOwner 或 IHasOrgUnit 的聚合根能声明 HasDataScope。");
        }

        if (!scopeObjects.Contains(scopeObject))
        {
            problems.Add($"{name}：范围对象 {scopeObject} 没有声明（[DataScopeObjectGroup] 中的常量）。");
        }
        else if (!scopeObject.StartsWith(module + ".", StringComparison.Ordinal))
        {
            problems.Add($"{name}：范围对象 {scopeObject} 属于其他模块，只能使用本模块 {module} 的范围对象。");
        }
    }

    // 复杂类型（值对象）的成员同样要满足规则
    private static IEnumerable<IReadOnlyProperty> AllProperties(IReadOnlyTypeBase type) =>
        type.GetDeclaredProperties().Concat(type.GetDeclaredComplexProperties().SelectMany(c => AllProperties(c.ComplexType)));
}
