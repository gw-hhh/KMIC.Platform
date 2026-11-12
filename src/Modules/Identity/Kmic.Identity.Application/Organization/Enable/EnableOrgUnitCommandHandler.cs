using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Enable;

/// <summary>
/// 处理 <see cref="EnableOrgUnitCommand"/>
/// </summary>
public sealed class EnableOrgUnitCommandHandler(IOrgTreeRepository trees, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<EnableOrgUnitCommand, VersionedResponse<OrgTreeId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<OrgTreeId>>> ExecuteAsync(EnableOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await trees.GetForUpdateAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        return AuthorizationChangeEvents.Complete(securityEvents, loaded.Value.Enable(command.Id), "OrgTree", loaded.Value.Id, "OrgUnitEnabled");
    }
}
