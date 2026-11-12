using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants.GrantRole;

/// <summary>
/// 处理 <see cref="GrantRoleCommand"/>
/// </summary>
public sealed class GrantRoleCommandHandler(
    IUserRepository users,
    IIdentityQueryDb db,
    IResourceAuthorizer<GrantTarget, GrantAction> authorizer,
    ISecurityEventWriter securityEvents,
    TimeProvider time,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<GrantRoleCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(GrantRoleCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await GrantLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, command.RoleId, GrantAction.Grant, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        if (!loaded.Value.RoleIsActive)
        {
            return IdentityErrors.RoleDisabled;
        }

        var anchors = await OrgAnchors.CheckAsync(db, command.Anchors, cancellationToken);
        if (anchors.IsFailure)
        {
            return anchors.Error;
        }

        var user = loaded.Value.User;
        var granted = user.Grant(command.RoleId, access.UserId, time.GetUtcNow(), command.Anchors);
        return AuthorizationChangeEvents.Complete(securityEvents, granted.IsFailure ? granted.Error : Result.Success(), "User", user.Id, "RoleGranted", GrantLoading.RoleDetails(command.RoleId));
    }
}
