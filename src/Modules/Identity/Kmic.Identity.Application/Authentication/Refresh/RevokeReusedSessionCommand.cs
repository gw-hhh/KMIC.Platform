using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 刷新令牌被重用时作废会话
/// </summary>
[AllowAnonymous]
[NotAudited("通过 RefreshTokenReused 安全事件记录令牌重用")]
[NotIdempotent("会话不存在或已作废时直接返回成功，不重复记录事件")]
public sealed record RevokeReusedSessionCommand(string RefreshToken) : ICommand;
