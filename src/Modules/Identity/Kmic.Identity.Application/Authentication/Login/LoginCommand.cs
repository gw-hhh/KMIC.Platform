using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Login;

/// <summary>
/// 账号密码登录
/// </summary>
/// <remarks>
/// 登录的结果是新的凭据，重试会产生新会话，所以不用幂等键，靠限流和登录失败限制防止滥用。
/// 结果记入安全事件，不写审计。账号不存在、密码错误、账号已禁用对外都是同一个错误，防止枚举账号
/// </remarks>
/// <param name="Account">登录名</param>
/// <param name="Password">密码</param>
/// <param name="ClientType">客户端类型：web 或 app</param>
/// <param name="ClientIp">客户端 IP，由接口层取得</param>
/// <param name="UserAgent">客户端 User-Agent，由接口层取得</param>
[AllowAnonymous]
[NotAudited("登录结果记入安全事件")]
[NotIdempotent("登录的结果是新的凭据，重试会产生新会话，靠限流和登录失败限制防止滥用")]
public sealed record LoginCommand(string Account, string Password, string ClientType, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
