using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据范围的计算规则，数据过滤和授权诊断共用，两边的结论才一致
/// </summary>
public static class DataScopeRules
{
    /// <summary>
    /// 范围对象的等级：角色为它单独设置了等级就用单独的，否则用角色的默认等级
    /// </summary>
    public static ScopeLevel LevelOf(DataScopeGrant grant, string scopeObject)
    {
        ArgumentNullException.ThrowIfNull(grant);

        return grant.Overrides.TryGetValue(scopeObject, out var overridden) ? overridden : grant.DefaultLevel;
    }

    /// <summary>
    /// 组织范围的起点：配置了锚点就用锚点，否则用用户所属组织
    /// </summary>
    public static IReadOnlyList<OrgUnitId> AnchorsOf(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);

        if (access.DataScopeGrant.Anchors.Count > 0)
        {
            return access.DataScopeGrant.Anchors;
        }

        return access.OrgUnitId is { } own ? [own] : [];
    }

    /// <summary>
    /// 某个等级能看到的组织单元；本人和全部这两个等级不按组织判断，返回空
    /// </summary>
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
