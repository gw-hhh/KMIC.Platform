using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Application.Users;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Organization.AssignUser;

/// <summary>
/// 处理 <see cref="AssignUserOrgUnitCommand"/>
/// </summary>
public sealed class AssignUserOrgUnitCommandHandler(
    IUserRepository users,
    IOrgTreeRepository trees,
    IIdentityCommandReads db,
    IResourceAuthorizer<UserTarget, UserAdminAction> authorizer,
    ISecurityEventWriter securityEvents,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<AssignUserOrgUnitCommand, VersionedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<UserId>>> ExecuteAsync(AssignUserOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        // 锁住组织树这一行再检查单元是否启用：停用单元的命令同样先锁树，不会在这边检查通过之后单元被停用
        var tree = await trees.GetForUpdateLockedAsync(OrgTreeId.Default, null, cancellationToken);
        if (tree.IsFailure)
        {
            return tree.Error;
        }

        if (command.OrgUnitId is { } unitId)
        {
            var unit = tree.Value.Find(unitId);
            if (unit is null)
            {
                return IdentityErrors.OrgUnitNotFound;
            }

            if (unit.Status != OrgUnitStatus.Active)
            {
                return IdentityErrors.OrgUnitDisabled;
            }
        }

        var loaded = await UserAdminLoading.LoadForAsync(users, db, authorizer, command.Id, command.ExpectedVersion, UserAdminAction.Edit, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        return AuthorizationChangeEvents.Complete(securityEvents, loaded.Value.AssignOrgUnit(command.OrgUnitId), "User", loaded.Value.Id, "UserOrgUnitAssigned");
    }
}
