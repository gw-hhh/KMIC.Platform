using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 声明测试身份的当前角色：权限点、默认范围等级与按范围对象的覆盖
/// </summary>
public sealed class TestRoleBuilder
{
    private readonly HashSet<string> _permissions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ScopeLevel> _overrides = new(StringComparer.Ordinal);
    private ScopeLevel _defaultLevel = ScopeLevel.Self;

    internal TestRoleBuilder()
    {
    }

    /// <summary>
    /// 增加权限点
    /// </summary>
    public TestRoleBuilder Permissions(params IEnumerable<string> permissions)
    {
        _permissions.UnionWith(permissions);
        return this;
    }

    /// <summary>
    /// 默认范围等级，不设置时为“本人”
    /// </summary>
    public TestRoleBuilder Scope(ScopeLevel level)
    {
        _defaultLevel = level;
        return this;
    }

    /// <summary>
    /// 某个范围对象上的等级
    /// </summary>
    public TestRoleBuilder Override(string scopeObject, ScopeLevel level)
    {
        _overrides[scopeObject] = level;
        return this;
    }

    internal IReadOnlySet<string> BuildPermissions() => _permissions.ToHashSet(StringComparer.Ordinal);

    internal DataScopeGrant BuildGrant(DataScopeGrant current) =>
        current with { DefaultLevel = _defaultLevel, Overrides = new Dictionary<string, ScopeLevel>(_overrides, StringComparer.Ordinal) };
}
