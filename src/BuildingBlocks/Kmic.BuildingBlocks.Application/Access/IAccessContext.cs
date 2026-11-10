namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 一次请求的访问上下文：每个请求从数据库加载状态，权限内容按授权版本缓存（ADR-0026）
/// </summary>
/// <remarks>
/// 结构定义在框架中，由 Identity 模块的加载器实现；授权行为与数据范围只读取它，不查库
/// </remarks>
public interface IAccessContext
{
    /// <summary>
    /// 用户；模拟身份时为被模拟的用户
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// 会话
    /// </summary>
    SessionId SessionId { get; }

    /// <summary>
    /// 当前角色
    /// </summary>
    RoleId RoleId { get; }

    /// <summary>
    /// 当前角色的授权版本，通过响应头返回给前端
    /// </summary>
    long AuthorizationVersion { get; }

    /// <summary>
    /// 当前角色的权限点编码
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    /// 用户所属组织单元；未分配时为空
    /// </summary>
    OrgUnitId? OrgUnitId { get; }

    /// <summary>
    /// 数据范围授予
    /// </summary>
    DataScopeGrant DataScopeGrant { get; }

    /// <summary>
    /// 当前树版本的组织树快照
    /// </summary>
    IOrgHierarchy OrgHierarchy { get; }

    /// <summary>
    /// 模拟身份时的实际操作人，否则为空
    /// </summary>
    UserId? ImpersonatorUserId { get; }

    /// <summary>
    /// 模拟会话是否允许修改；非模拟会话恒为 true
    /// </summary>
    bool ImpersonationAllowsWrite { get; }

    /// <summary>
    /// 是否必须先修改密码
    /// </summary>
    bool PasswordChangeRequired { get; }
}
