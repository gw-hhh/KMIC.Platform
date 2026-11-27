using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Login;

/// <summary>账号密码登录（各类失败统一返回登录失败，避免泄露账号存在性）</summary>
/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="ClientIp">客户端 IP</param>
/// <param name="UserAgent">客户端标识（User-Agent）</param>
[AllowAnonymous]
[NotAudited("登录结果记入安全事件")]
[NotIdempotent("每次成功登录都会创建新会话，令牌不写入幂等记录")]
[ExemptFromMessagingSuspension]
public sealed record LoginCommand(string Account, string Password, string ClientType, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>
{
    // 禁用编译器生成的 ToString 实现，避免在日志中输出密码明文
    public override string ToString() => nameof(LoginCommand);
}
