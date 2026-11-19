using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户的状态
/// </summary>
/// <remarks>
/// 登录失败造成的锁定不算用户状态
/// </remarks>
public sealed class UserStatus : Enumeration<UserStatus>
{
    /// <summary>
    /// 启用
    /// </summary>
    public static readonly UserStatus Active = new("Active", "启用");

    /// <summary>
    /// 禁止登录，已有会话全部作废
    /// </summary>
    public static readonly UserStatus Disabled = new("Disabled", "已禁用");

    private UserStatus(string code, string name)
        : base(code, name)
    {
    }
}
