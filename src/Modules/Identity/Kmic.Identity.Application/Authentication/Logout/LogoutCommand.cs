using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Logout;

/// <summary>
/// 退出登录：作废当前会话
/// </summary>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("通过 SessionRevoked 安全事件记录会话撤销")]
[NotIdempotent("撤销会话会保留已有撤销信息，无需缓存退出响应")]
[ExemptFromMessagingSuspension]
public sealed record LogoutCommand : ICommand;
