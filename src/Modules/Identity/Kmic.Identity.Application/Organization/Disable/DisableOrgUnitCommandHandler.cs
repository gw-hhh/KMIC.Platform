using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Organization.Disable;

/// <summary>
/// 处理 <see cref="DisableOrgUnitCommand"/>
/// </summary>
public sealed class DisableOrgUnitCommandHandler(IOrgTreeRepository trees, IIdentityCommandReads db, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<DisableOrgUnitCommand, VersionedResponse<OrgTreeId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<OrgTreeId>>> ExecuteAsync(DisableOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        // 先锁住组织树这一行再查归属的用户：把用户分配到这个单元的命令同样先锁树，两边排队，不会一边停用、一边又放进了人
        var loaded = await trees.GetForUpdateLockedAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var hasEnabledUsers = await db.Users.AnyAsync(u => u.OrgUnitId == command.Id && u.Status == UserStatus.Active, cancellationToken);
        return AuthorizationChangeEvents.Complete(securityEvents, loaded.Value.Disable(command.Id, hasEnabledUsers), "OrgTree", loaded.Value.Id, "OrgUnitDisabled");
    }
}
