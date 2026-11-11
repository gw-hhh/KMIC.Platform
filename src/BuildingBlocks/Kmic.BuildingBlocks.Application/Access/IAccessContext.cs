using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 访问上下文
/// </summary>
/// <remarks>
/// 由 Identity 模块加载，每个请求一份。授权和数据权限只读它，不再查库
/// </remarks>
public interface IAccessContext
{
    /// <summary>
    /// 用户
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// 会话
    /// </summary>
    SessionId SessionId { get; }

    /// <summary>
    /// 角色
    /// </summary>
    RoleId RoleId { get; }

    /// <summary>
    /// 授权版本号
    /// </summary>
    long AuthorizationVersion { get; }

    /// <summary>
    /// 权限点
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    /// 所属组织
    /// </summary>
    OrgUnitId? OrgUnitId { get; }

    /// <summary>
    /// 数据权限授予
    /// </summary>
    DataScopeGrant DataScopeGrant { get; }

    /// <summary>
    /// 组织树
    /// </summary>
    IOrgHierarchy OrgHierarchy { get; }

    /// <summary>
    /// 实际操作人
    /// </summary>
    UserId? ImpersonatorUserId { get; }

    /// <summary>
    /// 是否允许修改数据
    /// </summary>
    bool ImpersonationAllowsWrite { get; }

    /// <summary>
    /// 是否必须先修改密码
    /// </summary>
    bool PasswordChangeRequired { get; }
}
