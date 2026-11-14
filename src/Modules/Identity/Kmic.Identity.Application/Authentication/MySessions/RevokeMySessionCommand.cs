using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 结束当前用户的指定登录会话
/// </summary>
[AllowAuthenticated]
[NotAudited("通过 SessionRevoked 安全事件记录会话撤销")]
[NotIdempotent("会话已撤销时直接返回成功，不会重复记录撤销事件")]
public sealed record RevokeMySessionCommand(SessionId SessionId) : ICommand;
