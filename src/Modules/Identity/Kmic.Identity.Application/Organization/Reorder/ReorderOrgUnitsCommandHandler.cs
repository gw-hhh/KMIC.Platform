using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Reorder;

/// <summary>
/// 处理 <see cref="ReorderOrgUnitsCommand"/>
/// </summary>
public sealed class ReorderOrgUnitsCommandHandler(IOrgTreeRepository trees, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<ReorderOrgUnitsCommand, VersionedResponse<OrgTreeId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<OrgTreeId>>> ExecuteAsync(ReorderOrgUnitsCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await trees.GetForUpdateAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        return AuthorizationChangeEvents.Complete(securityEvents, loaded.Value.Reorder(command.ParentId, command.OrderedChildren), "OrgTree", loaded.Value.Id, "OrgUnitsReordered");
    }
}
