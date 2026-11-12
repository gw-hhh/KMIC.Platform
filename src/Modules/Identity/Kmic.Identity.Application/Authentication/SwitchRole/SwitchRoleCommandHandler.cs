using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authentication.SwitchRole;

/// <summary>
/// 处理 <see cref="SwitchRoleCommand"/>
/// </summary>
public sealed class SwitchRoleCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IIdentityCommandReads db,
    IExecutionContext execution,
    IAccessTokenIssuer tokens,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<SwitchRoleCommand, AuthTokens>
{
    /// <inheritdoc />
    public async ValueTask<Result<AuthTokens>> Handle(SwitchRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("切换角色需要已登录的访问上下文。");
        var loadedSession = await sessions.GetForUpdateAsync(access.SessionId, null, cancellationToken);
        var loadedUser = await users.GetForUpdateAsync(access.UserId, null, cancellationToken);
        if (loadedSession.IsFailure || loadedUser.IsFailure)
        {
            return IdentityErrors.RoleNotSwitchable;
        }

        // 目标角色必须是用户拥有的、启用的角色
        var user = loadedUser.Value;
        var active = user.HasRole(command.RoleId)
            && await db.Roles.AnyAsync(r => r.Id == command.RoleId && r.Status == RoleStatus.Active, cancellationToken);
        if (!active || loadedSession.Value.IsImpersonation)
        {
            return IdentityErrors.RoleNotSwitchable;
        }

        var session = loadedSession.Value;
        session.SwitchRole(command.RoleId);
        user.RecordRoleUsed(command.RoleId);
        securityEvents.Write(new SecurityEvent("RoleSwitched", SecuritySeverity.Info, null) { ResourceType = nameof(Role), ResourceId = command.RoleId.Value });

        var now = time.GetUtcNow();
        var accessToken = tokens.Issue(user.Id, session.Id, command.RoleId, ClientKinds.Claim(session.ClientType), now);
        return new AuthTokens(accessToken.Value, "Bearer", (int)(accessToken.ExpiresAt - now).TotalSeconds, null, user.PasswordChangeRequired);
    }
}
