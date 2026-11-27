namespace Kmic.Identity.Infrastructure.Access;

// 一次读取会话、用户、授予及角色和组织树并发版本号
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
