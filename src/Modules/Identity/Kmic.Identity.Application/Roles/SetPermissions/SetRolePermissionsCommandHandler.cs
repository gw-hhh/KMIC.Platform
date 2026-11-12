using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.SetPermissions;

/// <summary>
/// 处理 <see cref="SetRolePermissionsCommand"/>
/// </summary>
public sealed class SetRolePermissionsCommandHandler(
    IRoleRepository roles,
    IIdentityCommandReads db,
    IResourceAuthorizer<RoleTarget, RoleAdminAction> authorizer,
    PermissionCatalog catalog,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<SetRolePermissionsCommand, VersionedResponse<RoleId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<RoleId>>> ExecuteAsync(SetRolePermissionsCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var unknown = command.Permissions.Where(code => !catalog.Contains(code)).Distinct(StringComparer.Ordinal).ToList();
        if (unknown.Count > 0)
        {
            return IdentityErrors.PermissionUnknown.WithDetail($"不认识的权限点：{string.Join("、", unknown)}");
        }

        var loaded = await RoleAdminLoading.LoadForAsync(roles, db, authorizer, command.Id, command.ExpectedVersion, RoleAdminAction.SetPermissions, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var set = loaded.Value.SetPermissions(command.Permissions);
        return AuthorizationChangeEvents.Complete(securityEvents, set, "Role", loaded.Value.Id, "RolePermissionsSet");
    }
}
