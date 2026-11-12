using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 一个请求或作业的访问上下文
/// </summary>
internal sealed record AccessContext : IAccessContext
{
    public required UserId UserId { get; init; }

    public required SessionId SessionId { get; init; }

    public required RoleId RoleId { get; init; }

    public required long AuthorizationVersion { get; init; }

    public required IReadOnlySet<string> Permissions { get; init; }

    public OrgUnitId? OrgUnitId { get; init; }

    public required DataScopeGrant DataScopeGrant { get; init; }

    public required IOrgHierarchy OrgHierarchy { get; init; }

    public UserId? ImpersonatorUserId { get; init; }

    public bool ImpersonationAllowsWrite { get; init; }

    public bool PasswordChangeRequired { get; init; }
}
