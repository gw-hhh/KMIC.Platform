using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

public sealed class RoleStatus : Enumeration<RoleStatus>
{
    public static readonly RoleStatus Active = new("Active", "启用");

    /// <summary>已停用，不能再授予用户。</summary>
    public static readonly RoleStatus Disabled = new("Disabled", "已停用");

    private RoleStatus(string code, string name)
        : base(code, name)
    {
    }
}
