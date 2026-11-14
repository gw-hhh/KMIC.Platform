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
/// 停用组织单元
/// </summary>
public sealed class DisableOrgUnitCommandHandler(IOrgTreeRepository trees, IIdentityCommandReads db, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<DisableOrgUnitCommand, VersionedResponse<OrgTreeId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<OrgTreeId>>> ExecuteAsync(DisableOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        // 与用户组织分配操作使用同一组织树锁，防止停用检查期间新增所属用户
        var loaded = await trees.GetForUpdateLockedAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var hasEnabledUsers = await db.Users.AnyAsync(u => u.OrgUnitId == command.Id && u.Status == UserStatus.Active, cancellationToken);
        return AuthorizationChangeEvents.Complete(securityEvents, loaded.Value.Disable(command.Id, hasEnabledUsers), "OrgTree", loaded.Value.Id, "OrgUnitDisabled");
    }
}
