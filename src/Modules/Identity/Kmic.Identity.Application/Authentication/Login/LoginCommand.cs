using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Login;

/// <summary>
/// 账号密码登录
/// </summary>
/// <remarks>
/// 账号不存在、密码错误或账号禁用统一返回登录失败，避免泄露账号是否存在
/// </remarks>
/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型：web 或 app</param>
/// <param name="ClientIp">客户端 IP，由接口层取得</param>
/// <param name="UserAgent">客户端 User-Agent，由接口层取得</param>
[AllowAnonymous]
[NotAudited("登录结果记入安全事件")]
[NotIdempotent("成功登录会创建新会话并签发新凭据，防滥用由限流和登录失败限制负责")]
public sealed record LoginCommand(string Account, string Password, string ClientType, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
