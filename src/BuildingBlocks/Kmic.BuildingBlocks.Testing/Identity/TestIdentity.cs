using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>测试身份构建器，直接构造访问上下文。</summary>
/// <example>
/// <code>
/// var actor = TestIdentity.User("销售甲")
///     .InTree(org).InOrgUnit(org["上海"])
///     .WithRole(r => r.Permissions(SamplePermissions.View).Scope(ScopeLevel.OrgUnitAndBelow))
///     .WithAnchors(org["华东"]);
/// </code>
/// </example>
public sealed record TestIdentity : IAccessContext
{
    private TestIdentity(string displayName) => DisplayName = displayName;

    public string DisplayName { get; }

    public UserId UserId { get; init; } = UserId.New();

    public SessionId SessionId { get; init; } = SessionId.New();

    public RoleId RoleId { get; init; } = RoleId.New();

    public long AuthorizationVersion { get; init; } = 1;

    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    public OrgUnitId? OrgUnitId { get; init; }

    public DataScopeGrant DataScopeGrant { get; init; } = new(
        ScopeLevel.Self,
        new Dictionary<string, ScopeLevel>(StringComparer.Ordinal),
        [],
        new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.Ordinal));

    public IOrgHierarchy OrgHierarchy { get; init; } = TestOrgTree.Empty;

    public UserId? ImpersonatorUserId { get; init; }

    public bool ImpersonationAllowsWrite { get; init; } = true;

    public bool PasswordChangeRequired { get; init; }

    /// <summary>创建新用户（默认无权限点、仅本人数据范围且不属于任何组织）。</summary>
    public static TestIdentity User(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new TestIdentity(displayName);
    }

    /// <summary>指定关联的组织树。</summary>
    public TestIdentity InTree(IOrgHierarchy tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        return this with { OrgHierarchy = tree };
    }

    /// <summary>指定所属组织单元（须位于当前组织树内）。</summary>
    public TestIdentity InOrgUnit(OrgUnitId orgUnit) =>
        OrgHierarchy.Exists(orgUnit)
            ? this with { OrgUnitId = orgUnit }
            : throw new ArgumentException("组织单元不在当前测试组织树中，请先调用 InTree。", nameof(orgUnit));

    /// <summary>切换为新角色，自动更新角色 ID 与授权版本号，并保留原有范围起点。</summary>
    public TestIdentity WithRole(Action<TestRoleBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var role = new TestRoleBuilder();
        configure(role);
        return this with
        {
            RoleId = RoleId.New(),
            AuthorizationVersion = AuthorizationVersion + 1,
            Permissions = role.BuildPermissions(),
            DataScopeGrant = role.BuildGrant(DataScopeGrant),
        };
    }

    /// <summary>指定数据权限的范围起点组织（覆盖默认所属组织）。</summary>
    public TestIdentity WithAnchors(params IEnumerable<OrgUnitId> anchors) =>
        this with { DataScopeGrant = DataScopeGrant with { Anchors = [.. anchors] } };

    /// <summary>配置模拟登录（代管）状态。</summary>
    public TestIdentity ImpersonatedBy(UserId impersonator, bool allowWrite = false) =>
        this with { ImpersonatorUserId = impersonator, ImpersonationAllowsWrite = allowWrite };

    /// <summary>标记用户必须先修改密码。</summary>
    public TestIdentity MustChangePassword() => this with { PasswordChangeRequired = true };

    /// <summary>返回清空角色权限后的身份副本，用于模拟当前角色被收回后的用户上下文。</summary>
    public TestIdentity WithoutRolePrivileges() =>
        this with
        {
            Permissions = new HashSet<string>(StringComparer.Ordinal),
            DataScopeGrant = new DataScopeGrant(
                ScopeLevel.Self,
                new Dictionary<string, ScopeLevel>(StringComparer.Ordinal),
                [],
                new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.Ordinal)),
        };

    public override string ToString() => DisplayName;
}
