using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Roles;

public sealed class RoleAction : Enumeration<RoleAction>
{
    public static readonly RoleAction Disable = new("Disable", "停用");

    public static readonly RoleAction Enable = new("Enable", "启用");

    private RoleAction(string code, string name)
        : base(code, name)
    {
    }
}
