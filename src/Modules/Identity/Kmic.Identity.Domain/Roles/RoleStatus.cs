using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色的状态
/// </summary>
public sealed class RoleStatus : Enumeration<RoleStatus>
{
    /// <summary>
    /// 启用
    /// </summary>
    public static readonly RoleStatus Active = new("Active", "启用");

    /// <summary>
    /// 已停用，不能再授予
    /// </summary>
    public static readonly RoleStatus Disabled = new("Disabled", "已停用");

    private RoleStatus(string code, string name)
        : base(code, name)
    {
    }
}
