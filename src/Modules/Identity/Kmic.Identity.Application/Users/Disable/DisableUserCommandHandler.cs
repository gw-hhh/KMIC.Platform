using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Disable;

/// <summary>
/// 处理 <see cref="DisableUserCommand"/>
/// </summary>
public sealed class DisableUserCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<DisableUserCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(DisableUserCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, UserAdminAction.Disable, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var done = loaded.Value.Disable(command.Reason);
        return done.IsFailure ? done.Error : new VersionedResponse<UserId>(loaded.Value.Id);
    }
}
