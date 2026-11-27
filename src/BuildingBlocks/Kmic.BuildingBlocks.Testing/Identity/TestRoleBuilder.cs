using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>测试角色构建器。</summary>
public sealed class TestRoleBuilder
{
    private readonly HashSet<string> _permissions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ScopeLevel> _overrides = new(StringComparer.Ordinal);
    private ScopeLevel _defaultLevel = ScopeLevel.Self;

    internal TestRoleBuilder()
    {
    }

    /// <summary>添加权限点。</summary>
    public TestRoleBuilder Permissions(params IEnumerable<string> permissions)
    {
        _permissions.UnionWith(permissions);
        return this;
    }

    /// <summary>设置默认数据权限范围，未设置时仅限本人数据。</summary>
    public TestRoleBuilder Scope(ScopeLevel level)
    {
        _defaultLevel = level;
        return this;
    }

    /// <summary>设置指定范围对象的数据权限覆盖等级。</summary>
    public TestRoleBuilder Override(string scopeObject, ScopeLevel level)
    {
        _overrides[scopeObject] = level;
        return this;
    }

    internal IReadOnlySet<string> BuildPermissions() => _permissions.ToHashSet(StringComparer.Ordinal);

    internal DataScopeGrant BuildGrant(DataScopeGrant current) =>
        current with { DefaultLevel = _defaultLevel, Overrides = new Dictionary<string, ScopeLevel>(_overrides, StringComparer.Ordinal) };
}
