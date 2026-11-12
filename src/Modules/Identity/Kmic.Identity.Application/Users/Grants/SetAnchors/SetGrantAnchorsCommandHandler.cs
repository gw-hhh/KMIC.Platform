using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants.SetAnchors;

/// <summary>
/// 处理 <see cref="SetGrantAnchorsCommand"/>
/// </summary>
public sealed class SetGrantAnchorsCommandHandler(
    IUserRepository users,
    IIdentityQueryDb db,
    IResourceAuthorizer<GrantTarget, GrantAction> authorizer,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<SetGrantAnchorsCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(SetGrantAnchorsCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await GrantLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, command.RoleId, GrantAction.SetAnchors, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var anchors = await OrgAnchors.CheckAsync(db, command.Anchors, cancellationToken);
        if (anchors.IsFailure)
        {
            return anchors.Error;
        }

        var user = loaded.Value.User;
        return AuthorizationChangeEvents.Complete(securityEvents, user.SetAnchors(command.RoleId, command.Anchors), "User", user.Id, "GrantAnchorsSet", GrantLoading.RoleDetails(command.RoleId));
    }
}
