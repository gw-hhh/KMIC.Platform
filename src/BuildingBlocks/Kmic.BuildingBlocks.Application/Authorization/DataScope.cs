using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Application.Authorization;

internal sealed class DataScope(IExecutionContext execution) : IDataScope
{
    private static readonly EffectiveScope _nothing = new(false, null, []);

    // 单次请求内可能多次读取，按范围对象缓存计算结果
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

        // 匿名请求无法访问任何受归属约束的数据
        if (execution.Access is not { } access)
        {
            return _nothing;
        }

        var level = DataScopeRules.LevelOf(access.DataScopeGrant, scopeObject);
        return level switch
        {
            ScopeLevel.All => EffectiveScope.Unrestricted,
            ScopeLevel.Self => new(false, access.UserId, []),
            ScopeLevel.OrgUnit or ScopeLevel.OrgUnitAndBelow => new(false, access.UserId, [.. DataScopeRules.OrgUnitsOf(access, level)]),
            _ => throw new InvalidOperationException($"未知的范围等级 {level}。"),
        };
    }
}
