using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户上会引起状态流转的动作
/// </summary>
public sealed class UserAction : Enumeration<UserAction>
{
    /// <summary>
    /// 禁用
    /// </summary>
    public static readonly UserAction Disable = new("Disable", "禁用");

    /// <summary>
    /// 启用
    /// </summary>
    public static readonly UserAction Enable = new("Enable", "启用");

    private UserAction(string code, string name)
        : base(code, name)
    {
    }
}
