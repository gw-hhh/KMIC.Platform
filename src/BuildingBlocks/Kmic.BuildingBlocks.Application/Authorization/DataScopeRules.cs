using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>数据范围计算规则，供数据过滤与授权诊断共用以保证判定一致。</summary>
public static class DataScopeRules
{
    /// <summary>获取角色对指定范围对象的有效数据范围等级。</summary>
    public static ScopeLevel LevelOf(DataScopeGrant grant, string scopeObject)
    {
        ArgumentNullException.ThrowIfNull(grant);

        return grant.Overrides.TryGetValue(scopeObject, out var overridden) ? overridden : grant.DefaultLevel;
    }

    /// <summary>获取组织范围起点；优先使用锚点，否则使用所属组织。</summary>
    public static IReadOnlyList<OrgUnitId> AnchorsOf(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);

        if (access.DataScopeGrant.Anchors.Count > 0)
        {
            return access.DataScopeGrant.Anchors;
        }

        return access.OrgUnitId is { } own ? [own] : [];
    }

    /// <summary>获取指定数据范围等级下可见的组织单元集合。</summary>
    public static IReadOnlyList<OrgUnitId> OrgUnitsOf(IAccessContext access, ScopeLevel level)
    {
        ArgumentNullException.ThrowIfNull(access);

        return level switch
        {
            ScopeLevel.OrgUnit => AnchorsOf(access),
            ScopeLevel.OrgUnitAndBelow => [.. AnchorsOf(access).SelectMany(access.OrgHierarchy.SelfAndDescendants).Distinct()],
            _ => [],
        };
    }
}
