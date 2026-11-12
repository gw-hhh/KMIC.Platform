using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户的状态
/// </summary>
/// <remarks>
/// 登录失败过多造成的锁定不是用户状态，只影响新的登录尝试，见 LoginThrottle
/// </remarks>
public sealed class UserStatus : Enumeration<UserStatus>
{
    /// <summary>
    /// 启用
    /// </summary>
    public static readonly UserStatus Active = new("Active", "启用");

    /// <summary>
    /// 已禁用：不能登录，已有会话全部作废；用户被大量业务数据引用，所以只禁用不删除
    /// </summary>
    public static readonly UserStatus Disabled = new("Disabled", "已禁用");

    private UserStatus(string code, string name)
        : base(code, name)
    {
    }
}
