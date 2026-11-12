namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 访问上下文的状态查询的一行：会话、用户、授予、角色版本和组织树版本，一次查询取回
/// </summary>
internal sealed class AccessStateRow
{
    public Guid? SessionUserId { get; init; }

    public string? SessionStatus { get; init; }

    public DateTimeOffset? AbsoluteExpiresAt { get; init; }

    public Guid? CurrentRoleId { get; init; }

    public Guid? ImpersonatorUserId { get; init; }

    public bool ImpersonationAllowsWrite { get; init; }

    public required string UserStatus { get; init; }

    public required string UserKind { get; init; }

    public Guid? OrgUnitId { get; init; }

    public bool PasswordChangeRequired { get; init; }

    public Guid? GrantId { get; init; }

    public string? RoleStatus { get; init; }

    public long? RoleVersion { get; init; }

    public required Guid[] AnchorIds { get; init; }

    public long? TreeVersion { get; init; }
}
