using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 确认刷新令牌被重用后作废所属会话，由接口层在刷新失败后单独发送
/// </summary>
/// <remarks>
/// 要重新确认令牌已轮换且超过宽限期，否则不动会话，避免误作废
/// </remarks>
[AllowAnonymous]
[NotAudited("通过 RefreshTokenReused 安全事件记录令牌重用")]
[NotIdempotent("会话不存在或已作废时直接返回成功，不重复记录事件")]
public sealed record RevokeReusedSessionCommand(string RefreshToken) : ICommand;
