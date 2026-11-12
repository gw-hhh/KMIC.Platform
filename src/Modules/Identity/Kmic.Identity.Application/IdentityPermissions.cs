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
}
