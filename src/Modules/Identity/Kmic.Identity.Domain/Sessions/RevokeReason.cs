using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Identity.Domain.Sessions;

public sealed class RevokeReason : Enumeration<RevokeReason>
{
    public static readonly RevokeReason Logout = new("Logout", "退出登录");

    /// <summary>用户自主修改密码后作废其余会话。</summary>
    public static readonly RevokeReason PasswordChanged = new("PasswordChanged", "修改了密码");

    public static readonly RevokeReason PasswordReset = new("PasswordReset", "密码被重置");

    public static readonly RevokeReason UserDisabled = new("UserDisabled", "账号被禁用");

    public static readonly RevokeReason ForcedByAdmin = new("ForcedByAdmin", "被管理员强制下线");

    /// <summary>旧刷新令牌在宽限期外被重用。</summary>
    public static readonly RevokeReason RefreshTokenReused = new("RefreshTokenReused", "刷新令牌被重复使用");

    public static readonly RevokeReason ImpersonationEnded = new("ImpersonationEnded", "模拟身份结束");

    private RevokeReason(string code, string name)
        : base(code, name)
    {
    }
}
