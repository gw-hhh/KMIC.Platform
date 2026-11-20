using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Login;

/// <summary>
/// 账号密码登录
/// </summary>
/// <remarks>
/// 各种失败统一返回登录失败，不泄露账号是否存在
/// </remarks>
/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型</param>
/// <param name="ClientIp">客户端 IP</param>
/// <param name="UserAgent">客户端 User-Agent</param>
[AllowAnonymous]
[NotAudited("登录结果记入安全事件")]
[NotIdempotent("每次成功登录都会创建新会话，令牌不写入幂等记录")]
[ExemptFromMessagingSuspension]
public sealed record LoginCommand(string Account, string Password, string ClientType, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>
{
    // 不用编译器生成的：它会把密码、令牌的值也打出来
    public override string ToString() => nameof(LoginCommand);
}
