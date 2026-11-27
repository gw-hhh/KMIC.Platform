using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>请求级访问上下文。</summary>
public interface IAccessContext
{
    UserId UserId { get; }

    SessionId SessionId { get; }

    RoleId RoleId { get; }

    long AuthorizationVersion { get; }

    IReadOnlySet<string> Permissions { get; }

    OrgUnitId? OrgUnitId { get; }

    DataScopeGrant DataScopeGrant { get; }

    IOrgHierarchy OrgHierarchy { get; }

    /// <summary>代办或模拟登录时的实际操作人。</summary>
    UserId? ImpersonatorUserId { get; }

    /// <summary>模拟登录期间是否允许写入。</summary>
    bool ImpersonationAllowsWrite { get; }

    /// <summary>是否必须先修改密码才能执行业务操作。</summary>
    bool PasswordChangeRequired { get; }
}
