using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

/// <summary>修改当前用户密码</summary>
/// <param name="OldPassword">旧密码，首次设置密码时为空</param>
/// <param name="NewPassword">新密码</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[NotAudited("通过 PasswordChanged 安全事件记录修改，密码不记入审计")]
[NotIdempotent("每次修改都须校验旧密码，首次设置密码除外")]
[ExemptFromMessagingSuspension]
public sealed record ChangePasswordCommand(string? OldPassword, string NewPassword) : ICommand
{
    // 禁用编译器生成的 ToString 实现，避免在日志中输出密码明文
    public override string ToString() => nameof(ChangePasswordCommand);
}
