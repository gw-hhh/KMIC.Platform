using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 管理角色时的动作：资源策略与详情里的“可用动作”都按它
/// </summary>
public sealed class RoleAdminAction : Enumeration<RoleAdminAction>
{
    /// <summary>
    /// 改名称、说明、次序
    /// </summary>
    public static readonly RoleAdminAction Edit = new("Edit", "编辑");

    /// <summary>
    /// 设置权限点
    /// </summary>
    public static readonly RoleAdminAction SetPermissions = new("SetPermissions", "设置权限点");

    /// <summary>
    /// 设置数据范围
    /// </summary>
    public static readonly RoleAdminAction SetDataScopes = new("SetDataScopes", "设置数据范围");

    /// <summary>
    /// 停用
    /// </summary>
    public static readonly RoleAdminAction Disable = new("Disable", "停用");

    /// <summary>
    /// 启用
    /// </summary>
    public static readonly RoleAdminAction Enable = new("Enable", "启用");

    private RoleAdminAction(string code, string name)
        : base(code, name)
    {
    }
}
