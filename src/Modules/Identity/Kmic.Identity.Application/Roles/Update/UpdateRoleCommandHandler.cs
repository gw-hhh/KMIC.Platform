using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Update;

/// <summary>
/// 处理 <see cref="UpdateRoleCommand"/>
/// </summary>
public sealed class UpdateRoleCommandHandler(
    IRoleRepository roles,
    IIdentityCommandReads db,
    IResourceAuthorizer<RoleTarget, RoleAdminAction> authorizer,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<UpdateRoleCommand, VersionedResponse<RoleId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<RoleId>>> ExecuteAsync(UpdateRoleCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await RoleAdminLoading.LoadForAsync(roles, db, authorizer, command.Id, command.ExpectedVersion, RoleAdminAction.Edit, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var role = loaded.Value;
        var changed = Result.FirstFailure(role.Rename(command.Name), role.Describe(command.Description));
        if (changed.IsFailure)
        {
            return changed.Error;
        }

        role.Reorder(command.SortOrder);
        return new VersionedResponse<RoleId>(role.Id);
    }
}
