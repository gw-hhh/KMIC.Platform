using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Identity.Application;

/// <summary>
/// Identity 模块的权限点
/// </summary>
/// <remarks>
/// 使用敏感权限点的命令须记录审计，查询须记录读审计。
/// 未被用例或资源动作使用的权限点会导致启动失败
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
    /// 创建、修改、启用或停用角色，配置权限点和数据范围
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
    /// 授予或收回角色，设置角色授予的组织锚点
    /// </summary>
    [Permission("授予与收回角色", Sensitive = true)]
    public const string UserGrant = "Identity.User.Grant";

    /// <summary>
    /// 开始模拟身份，默认只读
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
