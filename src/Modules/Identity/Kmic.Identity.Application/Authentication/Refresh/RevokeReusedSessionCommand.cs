using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 确认刷新令牌重用后作废所属会话
/// </summary>
/// <remarks>
/// 刷新命令发现重用后返回失败并回滚，由接口层随后发送此命令，在独立事务中作废会话。
/// 须重新确认令牌已被轮换且超过宽限期，未确认时不修改会话，避免误作废其他会话
/// </remarks>
[AllowAnonymous]
[NotAudited("通过 RefreshTokenReused 安全事件记录令牌重用")]
[NotIdempotent("会话不存在或已作废时直接返回成功，不重复记录事件")]
public sealed record RevokeReusedSessionCommand(string RefreshToken) : ICommand;
