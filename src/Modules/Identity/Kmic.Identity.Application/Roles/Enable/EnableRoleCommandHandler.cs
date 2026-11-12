using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Enable;

/// <summary>
/// 处理 <see cref="EnableRoleCommand"/>
/// </summary>
public sealed class EnableRoleCommandHandler(
    IRoleRepository roles,
    IIdentityQueryDb db,
    IResourceAuthorizer<RoleTarget, RoleAdminAction> authorizer,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<EnableRoleCommand, VersionedResponse<RoleId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<RoleId>>> ExecuteAsync(EnableRoleCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await RoleAdminLoading.LoadForAsync(roles, db, authorizer, command.Id, command.ExpectedVersion, RoleAdminAction.Enable, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var done = loaded.Value.Enable();
        return AuthorizationChangeEvents.Complete(securityEvents, done, "Role", loaded.Value.Id, "RoleEnabled");
    }
}
