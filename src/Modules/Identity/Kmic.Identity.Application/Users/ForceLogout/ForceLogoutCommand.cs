using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>强制结束用户的指定或全部会话（含相关模拟会话）。</summary>
/// <param name="Id">用户 ID</param>
/// <param name="SessionId">目标会话 ID</param>
[RequirePermission(IdentityPermissions.UserForceLogout)]
[Audited]
[Idempotent]
public sealed record ForceLogoutCommand(UserId Id, SessionId? SessionId) : ICommand<VersionedResponse<UserId>>;
