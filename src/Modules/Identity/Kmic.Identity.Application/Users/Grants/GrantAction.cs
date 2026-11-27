using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>角色授予管理操作，供资源策略鉴权使用。</summary>
public sealed class GrantAction : Enumeration<GrantAction>
{
    public static readonly GrantAction Grant = new("Grant", "授予角色");

    public static readonly GrantAction Revoke = new("Revoke", "收回角色");

    /// <summary>设置角色授予的组织锚点。</summary>
    public static readonly GrantAction SetAnchors = new("SetAnchors", "设置锚点");

    private GrantAction(string code, string name)
        : base(code, name)
    {
    }
}
