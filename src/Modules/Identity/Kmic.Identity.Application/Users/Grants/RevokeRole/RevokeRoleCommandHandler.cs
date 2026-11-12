using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants.RevokeRole;

/// <summary>
/// 处理 <see cref="RevokeRoleCommand"/>
/// </summary>
public sealed class RevokeRoleCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    IResourceAuthorizer<GrantTarget, GrantAction> authorizer,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<RevokeRoleCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(RevokeRoleCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await GrantLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, command.RoleId, GrantAction.Revoke, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value.User;
        return AuthorizationChangeEvents.Complete(securityEvents, user.Revoke(command.RoleId), "User", user.Id, "RoleRevoked", GrantLoading.RoleDetails(command.RoleId));
    }
}
