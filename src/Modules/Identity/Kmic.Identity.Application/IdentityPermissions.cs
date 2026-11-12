using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Identity.Application;

/// <summary>
/// Identity 模块的权限点
/// </summary>
/// <remarks>
/// 资源名一律单数。标了敏感的权限点，用到它的命令必须审计，查询必须记读审计。
/// 框架启动时检查每个权限点都被用例或资源动作用到，所以权限点随用到它的用例一起加入
/// </remarks>
[PermissionGroup("Identity", "身份与授权")]
public static class IdentityPermissions
{
    /// <summary>
    /// 查看角色、权限点目录、范围对象
    /// </summary>
    [Permission("查看角色")]
    public const string RoleView = "Identity.Role.View";

    /// <summary>
    /// 角色的增改停用、权限点与范围配置
    /// </summary>
    [Permission("管理角色", Sensitive = true)]
    public const string RoleManage = "Identity.Role.Manage";

    /// <summary>
    /// 查看用户、会话
    /// </summary>
    [Permission("查看用户")]
    public const string UserView = "Identity.User.View";

    /// <summary>
    /// 新建、修改资料、禁用、启用用户
    /// </summary>
    [Permission("管理用户")]
    public const string UserManage = "Identity.User.Manage";

    /// <summary>
    /// 管理员重置用户密码
    /// </summary>
    [Permission("重置用户密码", Sensitive = true)]
    public const string UserResetPassword = "Identity.User.ResetPassword";

    /// <summary>
    /// 解锁被登录限制的账号
    /// </summary>
    [Permission("解锁登录", Sensitive = true)]
    public const string UserUnlock = "Identity.User.Unlock";

    /// <summary>
    /// 强制用户下线
    /// </summary>
    [Permission("强制下线", Sensitive = true)]
    public const string UserForceLogout = "Identity.User.ForceLogout";

    /// <summary>
    /// 维护组织树与用户归属
    /// </summary>
    [Permission("管理组织", Sensitive = true)]
    public const string OrgUnitManage = "Identity.OrgUnit.Manage";

    /// <summary>
    /// 授予、收回用户的角色与锚点
    /// </summary>
    [Permission("授予与收回角色", Sensitive = true)]
    public const string UserGrant = "Identity.User.Grant";

    /// <summary>
    /// 模拟身份（只读）
    /// </summary>
    [Permission("模拟身份", Sensitive = true)]
    public const string UserImpersonate = "Identity.User.Impersonate";

    /// <summary>
    /// 模拟身份时允许修改数据
    /// </summary>
    [Permission("模拟身份时允许写入", Sensitive = true)]
    public const string UserImpersonateWrite = "Identity.User.ImpersonateWrite";

    /// <summary>
    /// 查看安全事件
    /// </summary>
    [Permission("查看安全事件", Sensitive = true)]
    public const string SecurityEventView = "Identity.SecurityEvent.View";
}
