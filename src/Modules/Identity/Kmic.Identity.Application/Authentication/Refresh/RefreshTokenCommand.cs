using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 用刷新令牌换新的访问令牌与刷新令牌
/// </summary>
/// <remarks>
/// 超出宽限期重用旧令牌会作废会话
/// </remarks>
/// <param name="RefreshToken">客户端出示的刷新令牌</param>
/// <param name="ClientIp">客户端 IP</param>
[AllowAnonymous]
[NotAudited("仅轮换会话凭据，令牌重用通过安全事件记录")]
[NotIdempotent("成功刷新会轮换令牌，旧令牌重试受宽限期限制")]
public sealed record RefreshTokenCommand(string RefreshToken, string? ClientIp) : ICommand<AuthTokens>;
