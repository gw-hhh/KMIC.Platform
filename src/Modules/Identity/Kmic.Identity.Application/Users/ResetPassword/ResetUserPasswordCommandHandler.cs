using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 处理 <see cref="ResetUserPasswordCommand"/>
/// </summary>
public sealed class ResetUserPasswordCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    IPasswordService passwords,
    TemporaryPasswordGenerator generator,
    ISecurityEventWriter securityEvents,
    TimeProvider time,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<ResetUserPasswordCommand, ResetUserPasswordResponse>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<ResetUserPasswordResponse>> ExecuteAsync(ResetUserPasswordCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, null, UserAdminAction.ResetPassword, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value;
        var temporary = generator.Generate();
        var set = user.SetPassword(passwords.Hash(user, temporary), mustChange: true, byAdministrator: true, time.GetUtcNow());
        if (set.IsFailure)
        {
            return set.Error;
        }

        securityEvents.Write(new SecurityEvent("PasswordReset", SecuritySeverity.Warning, "ByAdministrator") { ResourceType = "User", ResourceId = user.Id.Value });
        return new ResetUserPasswordResponse(user.Id, temporary);
    }
}
