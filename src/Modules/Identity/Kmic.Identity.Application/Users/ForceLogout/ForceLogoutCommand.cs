using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>
/// 结束用户的指定会话或全部会话，包括相关的模拟会话
/// </summary>
/// <param name="Id">用户</param>
/// <param name="SessionId">目标会话 ID</param>
[RequirePermission(IdentityPermissions.UserForceLogout)]
[Audited]
[Idempotent]
public sealed record ForceLogoutCommand(UserId Id, SessionId? SessionId) : ICommand<VersionedResponse<UserId>>;
