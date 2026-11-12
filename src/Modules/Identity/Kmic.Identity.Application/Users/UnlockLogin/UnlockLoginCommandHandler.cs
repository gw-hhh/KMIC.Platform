using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.UnlockLogin;

/// <summary>
/// 处理 <see cref="UnlockLoginCommand"/>
/// </summary>
public sealed class UnlockLoginCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    ILoginThrottleStore throttle,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<UnlockLoginCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(UnlockLoginCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, null, UserAdminAction.Unlock, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value;
        await throttle.ClearAsync(LoginNames.Hash(user.Account), cancellationToken);
        securityEvents.Write(new SecurityEvent("AccountUnlocked", SecuritySeverity.Info, null) { ResourceType = "User", ResourceId = user.Id.Value });
        return new VersionedResponse<UserId>(user.Id, user.ConcurrencyVersion);
    }
}
