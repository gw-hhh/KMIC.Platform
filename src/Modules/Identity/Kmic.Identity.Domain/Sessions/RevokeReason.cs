using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话被作废的原因
/// </summary>
public sealed class RevokeReason : Enumeration<RevokeReason>
{
    /// <summary>
    /// 用户退出
    /// </summary>
    public static readonly RevokeReason Logout = new("Logout", "退出登录");

    /// <summary>
    /// 用户自己修改了密码，其他会话作废
    /// </summary>
    public static readonly RevokeReason PasswordChanged = new("PasswordChanged", "修改了密码");

    /// <summary>
    /// 管理员或运维重置了密码
    /// </summary>
    public static readonly RevokeReason PasswordReset = new("PasswordReset", "密码被重置");

    /// <summary>
    /// 账号被禁用
    /// </summary>
    public static readonly RevokeReason UserDisabled = new("UserDisabled", "账号被禁用");

    /// <summary>
    /// 管理员强制下线
    /// </summary>
    public static readonly RevokeReason ForcedByAdmin = new("ForcedByAdmin", "被管理员强制下线");

    /// <summary>
    /// 旧的刷新令牌在宽限期之外再次出现，判定为被盗用
    /// </summary>
    public static readonly RevokeReason RefreshTokenReused = new("RefreshTokenReused", "刷新令牌被重复使用");

    /// <summary>
    /// 模拟身份结束
    /// </summary>
    public static readonly RevokeReason ImpersonationEnded = new("ImpersonationEnded", "模拟身份结束");

    private RevokeReason(string code, string name)
        : base(code, name)
    {
    }
}
