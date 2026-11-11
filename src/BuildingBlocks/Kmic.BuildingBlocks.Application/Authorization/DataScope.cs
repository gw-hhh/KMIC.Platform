using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据权限
/// </summary>
/// <remarks>
/// 一次查询会读取多次，结果按范围对象缓存在实例里
/// </remarks>
internal sealed class DataScope(IExecutionContext execution) : IDataScope
{
    private static readonly EffectiveScope _nothing = new(false, null, []);

    private readonly Dictionary<string, EffectiveScope> _computed = new(StringComparer.Ordinal);

    public bool Unrestricted => execution.User.IsSystem;

    public EffectiveScope For(string scopeObject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeObject);
        if (!_computed.TryGetValue(scopeObject, out var scope))
        {
            scope = Compute(scopeObject);
            _computed[scopeObject] = scope;
        }

        return scope;
    }

    private EffectiveScope Compute(string scopeObject)
    {
        if (execution.User.IsSystem)
        {
            return EffectiveScope.Unrestricted;
        }

        // 匿名请求看不到任何有归属的数据
        if (execution.Access is not { } access)
        {
            return _nothing;
        }

        var grant = access.DataScopeGrant;
        var level = grant.Overrides.TryGetValue(scopeObject, out var overridden) ? overridden : grant.DefaultLevel;
        return level switch
        {
            ScopeLevel.All => EffectiveScope.Unrestricted,
            ScopeLevel.Self => new(false, access.UserId, []),
            ScopeLevel.OrgUnit => new(false, access.UserId, [.. Anchors(access)]),
            ScopeLevel.OrgUnitAndBelow => new(false, access.UserId, [.. Anchors(access).SelectMany(access.OrgHierarchy.SelfAndDescendants).Distinct()]),
            _ => throw new InvalidOperationException($"未知的范围等级 {level}。"),
        };
    }

    // 配置了起点组织就从起点算，不用用户所属组织
    private static IReadOnlyList<OrgUnitId> Anchors(IAccessContext access)
    {
        if (access.DataScopeGrant.Anchors.Count > 0)
        {
            return access.DataScopeGrant.Anchors;
        }

        return access.OrgUnitId is { } own ? [own] : [];
    }
}
