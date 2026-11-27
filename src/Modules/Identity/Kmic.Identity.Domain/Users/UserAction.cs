using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

public sealed class UserAction : Enumeration<UserAction>
{
    public static readonly UserAction Disable = new("Disable", "禁用");

    public static readonly UserAction Enable = new("Enable", "启用");

    private UserAction(string code, string name)
        : base(code, name)
    {
    }
}
