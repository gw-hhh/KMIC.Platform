using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Enable;

/// <summary>
/// 处理 <see cref="EnableUserCommand"/>
/// </summary>
public sealed class EnableUserCommandHandler(
    IUserRepository users,
    IIdentityQueryDb db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<EnableUserCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(EnableUserCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, UserAdminAction.Enable, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var done = loaded.Value.Enable();
        return done.IsFailure ? done.Error : new VersionedResponse<UserId>(loaded.Value.Id);
    }
}
