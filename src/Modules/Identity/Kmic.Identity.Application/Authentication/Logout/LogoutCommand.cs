using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Logout;

/// <summary>
/// 退出登录：作废当前会话
/// </summary>
/// <remarks>
/// 天然幂等：会话已经作废时再次退出什么也不改变
/// </remarks>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("结果记入安全事件")]
[NotIdempotent("退出天然幂等：会话已经作废时再次退出不改变任何东西")]
public sealed record LogoutCommand : ICommand;
