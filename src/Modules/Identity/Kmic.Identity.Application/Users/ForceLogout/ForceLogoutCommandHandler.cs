using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>
/// 处理 <see cref="ForceLogoutCommand"/>
/// </summary>
public sealed class ForceLogoutCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IIdentityQueryDb db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    ISecurityEventWriter securityEvents,
    TimeProvider time,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<ForceLogoutCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(ForceLogoutCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, null, UserAdminAction.ForceLogout, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var active = await sessions.ListActiveInvolvingAsync(command.Id, cancellationToken);
        var targets = command.SessionId is { } only ? active.Where(s => s.Id == only).ToList() : [.. active];
        if (command.SessionId is not null && targets.Count == 0)
        {
            return IdentityErrors.SessionNotFound;
        }

        var now = time.GetUtcNow();
        foreach (var session in targets)
        {
            session.Revoke(RevokeReason.ForcedByAdmin, now);
            securityEvents.Write(new SecurityEvent("SessionRevoked", SecuritySeverity.Info, RevokeReason.ForcedByAdmin.Code) { ResourceType = "Session", ResourceId = session.Id.Value });
        }

        return new VersionedResponse<UserId>(command.Id, loaded.Value.ConcurrencyVersion);
    }
}
