namespace Kmic.Identity.Presentation.Authentication;

/// <param name="OldPassword">旧密码</param>
/// <param name="NewPassword">新密码</param>
public sealed record ChangePasswordRequest(string? OldPassword, string NewPassword)
{
    // 禁用编译器生成的实现，避免输出密码或令牌明文
    public override string ToString() => nameof(ChangePasswordRequest);
}
