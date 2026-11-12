using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.UpdateProfile;

/// <summary>
/// 处理 <see cref="UpdateUserProfileCommand"/>
/// </summary>
public sealed class UpdateUserProfileCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<UpdateUserProfileCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(UpdateUserProfileCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, UserAdminAction.Edit, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value;
        if (user.WouldChangeCredentials(command.Account, command.Mobile))
        {
            var allowed = authorizer.Check(await UserTargets.BuildAsync(db, user, access, cancellationToken), UserAdminAction.ChangeCredentials);
            if (allowed.IsFailure)
            {
                return allowed.Error;
            }
        }

        var updated = user.UpdateProfile(command.Account, command.DisplayName, command.EnglishName, command.EmployeeNo, command.Mobile, command.Email);
        return updated.IsFailure ? updated.Error : new VersionedResponse<UserId>(user.Id);
    }
}
