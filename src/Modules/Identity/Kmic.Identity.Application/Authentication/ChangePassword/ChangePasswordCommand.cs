using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

/// <summary>
/// 修改自己的密码
/// </summary>
/// <remarks>
/// 已经有密码时要给旧密码（管理员重置的临时密码也算旧密码）；还没有设置过密码的用户（迁移来的员工靠短信登录）
/// 不要求旧密码，这个会话刚刚通过验证码验证过手机号。成功后其他会话全部作废，当前会话继续。
/// 必须先改密码的用户只能做这件事、查看自己和退出
/// </remarks>
/// <param name="OldPassword">旧密码；还没有设置过密码时为空</param>
/// <param name="NewPassword">新密码</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[NotAudited("结果记入安全事件，密码不进审计")]
[NotIdempotent("改密码不是可以重放的操作：重放时旧密码已经不对")]
public sealed record ChangePasswordCommand(string? OldPassword, string NewPassword) : ICommand;
