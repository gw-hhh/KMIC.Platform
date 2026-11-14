using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

/// <summary>
/// 修改当前用户密码
/// </summary>
/// <param name="OldPassword">旧密码；还没有设置过密码时为空</param>
/// <param name="NewPassword">新密码</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[NotAudited("通过 PasswordChanged 安全事件记录修改，密码不进审计")]
[NotIdempotent("修改前须按当前密码状态校验，不能用缓存结果代替执行")]
public sealed record ChangePasswordCommand(string? OldPassword, string NewPassword) : ICommand;
