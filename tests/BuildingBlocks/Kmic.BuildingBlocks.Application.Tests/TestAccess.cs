using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Tests;

/// <summary>
/// 测试用的访问上下文
/// </summary>
internal sealed record TestAccess : IAccessContext
{
    public UserId UserId { get; init; } = UserId.New();

    public SessionId SessionId { get; init; } = SessionId.New();

    public RoleId RoleId { get; init; } = RoleId.New();

    public long AuthorizationVersion { get; init; } = 1;

    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    public OrgUnitId? OrgUnitId { get; init; }

    public DataScopeGrant DataScopeGrant { get; init; } =
        new(ScopeLevel.Self, new Dictionary<string, ScopeLevel>(StringComparer.Ordinal), [], new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.Ordinal));

    public IOrgHierarchy OrgHierarchy => throw new NotSupportedException("本组测试不涉及组织树");

    public UserId? ImpersonatorUserId { get; init; }

    public bool ImpersonationAllowsWrite { get; init; } = true;

    public bool PasswordChangeRequired { get; init; }

    public static TestAccess WithPermissions(params string[] permissions) =>
        new() { Permissions = new HashSet<string>(permissions, StringComparer.Ordinal) };
}
