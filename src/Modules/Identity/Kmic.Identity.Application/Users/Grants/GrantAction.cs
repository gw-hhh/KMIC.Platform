using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 管理角色授予时的动作：资源策略按它判断
/// </summary>
public sealed class GrantAction : Enumeration<GrantAction>
{
    /// <summary>
    /// 授予角色
    /// </summary>
    public static readonly GrantAction Grant = new("Grant", "授予角色");

    /// <summary>
    /// 收回角色
    /// </summary>
    public static readonly GrantAction Revoke = new("Revoke", "收回角色");

    /// <summary>
    /// 修改授予上的锚点
    /// </summary>
    public static readonly GrantAction SetAnchors = new("SetAnchors", "设置锚点");

    private GrantAction(string code, string name)
        : base(code, name)
    {
    }
}
