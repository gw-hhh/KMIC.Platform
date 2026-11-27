using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Roles;

/// <summary>角色管理操作，供资源策略和可用操作列表使用。</summary>
public sealed class RoleAdminAction : Enumeration<RoleAdminAction>
{
    /// <summary>修改角色名称、说明和次序。</summary>
    public static readonly RoleAdminAction Edit = new("Edit", "编辑");

    public static readonly RoleAdminAction SetPermissions = new("SetPermissions", "设置权限点");

    public static readonly RoleAdminAction SetDataScopes = new("SetDataScopes", "设置数据范围");

    public static readonly RoleAdminAction Disable = new("Disable", "停用");

    public static readonly RoleAdminAction Enable = new("Enable", "启用");

    private RoleAdminAction(string code, string name)
        : base(code, name)
    {
    }
}
