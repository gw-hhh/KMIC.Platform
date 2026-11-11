using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 测试身份
/// </summary>
/// <remarks>
/// 直接构造访问上下文，不经过登录。每个方法都返回新实例
/// </remarks>
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

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; }

    /// <inheritdoc />
    public UserId UserId { get; init; } = UserId.New();

    /// <inheritdoc />
    public SessionId SessionId { get; init; } = SessionId.New();

    /// <inheritdoc />
    public RoleId RoleId { get; init; } = RoleId.New();

    /// <inheritdoc />
    public long AuthorizationVersion { get; init; } = 1;

    /// <inheritdoc />
    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <inheritdoc />
    public OrgUnitId? OrgUnitId { get; init; }

    /// <inheritdoc />
    public DataScopeGrant DataScopeGrant { get; init; } = new(
        ScopeLevel.Self,
        new Dictionary<string, ScopeLevel>(StringComparer.Ordinal),
        [],
        new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.Ordinal));

    /// <inheritdoc />
    public IOrgHierarchy OrgHierarchy { get; init; } = TestOrgTree.Empty;

    /// <inheritdoc />
    public UserId? ImpersonatorUserId { get; init; }

    /// <inheritdoc />
    public bool ImpersonationAllowsWrite { get; init; } = true;

    /// <inheritdoc />
    public bool PasswordChangeRequired { get; init; }

    /// <summary>
    /// 新用户，没有权限点，只能看本人数据，不属于任何组织
    /// </summary>
    public static TestIdentity User(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new TestIdentity(displayName);
    }

    /// <summary>
    /// 使用的组织树
    /// </summary>
    public TestIdentity InTree(IOrgHierarchy tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        return this with { OrgHierarchy = tree };
    }

    /// <summary>
    /// 所属组织，必须在当前组织树里
    /// </summary>
    public TestIdentity InOrgUnit(OrgUnitId orgUnit) =>
        OrgHierarchy.Exists(orgUnit)
            ? this with { OrgUnitId = orgUnit }
            : throw new ArgumentException("组织单元不在当前测试组织树中，先调用 InTree。", nameof(orgUnit));

    /// <summary>
    /// 换成新角色，角色 ID 和授权版本号随之变化，起点组织保留
    /// </summary>
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

    /// <summary>
    /// 数据权限的起点组织，代替所属组织
    /// </summary>
    public TestIdentity WithAnchors(params IEnumerable<OrgUnitId> anchors) =>
        this with { DataScopeGrant = DataScopeGrant with { Anchors = [.. anchors] } };

    /// <summary>
    /// 模拟登录
    /// </summary>
    /// <param name="impersonator">实际操作人</param>
    /// <param name="allowWrite">是否允许修改数据</param>
    public TestIdentity ImpersonatedBy(UserId impersonator, bool allowWrite = false) =>
        this with { ImpersonatorUserId = impersonator, ImpersonationAllowsWrite = allowWrite };

    /// <summary>
    /// 需要先修改密码
    /// </summary>
    public TestIdentity MustChangePassword() => this with { PasswordChangeRequired = true };

    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
