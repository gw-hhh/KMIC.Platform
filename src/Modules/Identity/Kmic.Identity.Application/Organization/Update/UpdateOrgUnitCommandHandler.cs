using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Update;

/// <summary>
/// 处理 <see cref="UpdateOrgUnitCommand"/>
/// </summary>
public sealed class UpdateOrgUnitCommandHandler(IOrgTreeRepository trees, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<UpdateOrgUnitCommand, VersionedResponse<OrgTreeId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<OrgTreeId>>> ExecuteAsync(UpdateOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await trees.GetForUpdateAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var tree = loaded.Value;
        var updated = Result.FirstFailure(tree.Rename(command.Id, command.Name), tree.ChangeCode(command.Id, command.Code));
        return AuthorizationChangeEvents.Complete(securityEvents, updated, "OrgTree", tree.Id, "OrgUnitUpdated");
    }
}
