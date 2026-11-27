using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Identity.Application;

[PermissionGroup("Identity", "身份与授权")]
public static class IdentityPermissions
{
    /// <summary>查看角色、权限点目录与范围对象</summary>
    [Permission("查看角色")]
    public const string RoleView = "Identity.Role.View";

    /// <summary>创建、修改、启用或停用角色，配置权限点和数据范围</summary>
    [Permission("管理角色", Sensitive = true)]
    public const string RoleManage = "Identity.Role.Manage";

    /// <summary>查看用户与登录会话</summary>
    [Permission("查看用户")]
    public const string UserView = "Identity.User.View";

    /// <summary>新建、修改资料、禁用或启用用户</summary>
    [Permission("管理用户")]
    public const string UserManage = "Identity.User.Manage";

    /// <summary>管理员重置用户密码</summary>
    [Permission("重置用户密码", Sensitive = true)]
    public const string UserResetPassword = "Identity.User.ResetPassword";

    /// <summary>解锁被登录限流锁定的账号</summary>
    [Permission("解锁登录", Sensitive = true)]
    public const string UserUnlock = "Identity.User.Unlock";

    [Permission("强制下线", Sensitive = true)]
    public const string UserForceLogout = "Identity.User.ForceLogout";

    /// <summary>维护组织树与用户归属</summary>
    [Permission("管理组织", Sensitive = true)]
    public const string OrgUnitManage = "Identity.OrgUnit.Manage";

    /// <summary>授予或收回角色，设置授予的组织锚点</summary>
    [Permission("授予与收回角色", Sensitive = true)]
    public const string UserGrant = "Identity.User.Grant";

    /// <summary>开启模拟会话（默认只读）</summary>
    [Permission("模拟身份", Sensitive = true)]
    public const string UserImpersonate = "Identity.User.Impersonate";

    [Permission("模拟身份时允许写入", Sensitive = true)]
    public const string UserImpersonateWrite = "Identity.User.ImpersonateWrite";

    [Permission("查看安全事件", Sensitive = true)]
    public const string SecurityEventView = "Identity.SecurityEvent.View";

    /// <summary>查看用户在指定角色下的权限与数据范围</summary>
    [Permission("授权诊断", Sensitive = true)]
    public const string AuthorizationExplain = "Identity.Authorization.Explain";
}
