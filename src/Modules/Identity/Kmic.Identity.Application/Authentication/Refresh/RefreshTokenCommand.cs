using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 用刷新令牌换新的访问令牌与刷新令牌
/// </summary>
/// <remarks>
/// 每次刷新换一个新的刷新令牌，旧令牌宽限期内再次出现视为并发刷新，宽限期之外再次出现是重用。
/// 重用时会话必须作废并保留这个结果，而失败的命令会回滚，所以重用返回 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.RefreshTokenReused" />，
/// 接口层再发 <see cref="RevokeReusedSessionCommand" /> 在独立的事务里作废会话，对外统一是 401
/// </remarks>
/// <param name="RefreshToken">客户端出示的刷新令牌</param>
/// <param name="ClientIp">客户端 IP，由接口层取得</param>
[AllowAnonymous]
[NotAudited("刷新与重用结果记入安全事件")]
[NotIdempotent("刷新每次换新令牌，重试由宽限期处理")]
public sealed record RefreshTokenCommand(string RefreshToken, string? ClientIp) : ICommand<AuthTokens>;
