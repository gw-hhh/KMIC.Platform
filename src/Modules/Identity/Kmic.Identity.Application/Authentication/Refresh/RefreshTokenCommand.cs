using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>使用刷新令牌换发新令牌（超出宽限期重用旧令牌将作废会话）</summary>
/// <param name="RefreshToken">客户端出示的刷新令牌</param>
/// <param name="ClientIp">客户端 IP</param>
[AllowAnonymous]
[NotAudited("仅轮换会话凭据，令牌重用通过安全事件记录")]
[NotIdempotent("成功刷新会轮换令牌，旧令牌重试受宽限期限制")]
[ExemptFromMessagingSuspension]
public sealed record RefreshTokenCommand(string RefreshToken, string? ClientIp) : ICommand<AuthTokens>
{
    // 禁用编译器生成的 ToString 实现，避免在日志中输出令牌明文
    public override string ToString() => nameof(RefreshTokenCommand);
}
