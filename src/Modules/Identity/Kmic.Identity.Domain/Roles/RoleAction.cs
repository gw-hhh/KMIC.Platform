using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色上会引起状态流转的动作
/// </summary>
public sealed class RoleAction : Enumeration<RoleAction>
{
    /// <summary>
    /// 停用
    /// </summary>
    public static readonly RoleAction Disable = new("Disable", "停用");

    /// <summary>
    /// 启用
    /// </summary>
    public static readonly RoleAction Enable = new("Enable", "启用");

    private RoleAction(string code, string name)
        : base(code, name)
    {
    }
}
