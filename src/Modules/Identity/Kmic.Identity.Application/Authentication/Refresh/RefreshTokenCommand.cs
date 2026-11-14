using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 用刷新令牌换新的访问令牌与刷新令牌
/// </summary>
/// <remarks>
/// 旧令牌在宽限期内再次使用时按并发刷新处理；超出宽限期则返回 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.RefreshTokenReused" /> 并回滚。
/// 接口层再发送 <see cref="RevokeReusedSessionCommand" />，在独立事务中复查并作废会话，对外返回 401
/// </remarks>
/// <param name="RefreshToken">客户端出示的刷新令牌</param>
/// <param name="ClientIp">客户端 IP，由接口层取得</param>
[AllowAnonymous]
[NotAudited("响应包含访问令牌和刷新令牌，令牌重用另记安全事件")]
[NotIdempotent("刷新每次换新令牌，重试由宽限期处理")]
public sealed record RefreshTokenCommand(string RefreshToken, string? ClientIp) : ICommand<AuthTokens>;
