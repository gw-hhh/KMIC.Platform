using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

/// <summary>用户状态；登录失败引起的锁定不属于用户状态。</summary>
public sealed class UserStatus : Enumeration<UserStatus>
{
    public static readonly UserStatus Active = new("Active", "启用");

    /// <summary>禁止登录，已有会话全部作废。</summary>
    public static readonly UserStatus Disabled = new("Disabled", "已禁用");

    private UserStatus(string code, string name)
        : base(code, name)
    {
    }
}
