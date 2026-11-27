using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Create;

public sealed class CreateOrgUnitCommandHandler(IOrgTreeRepository trees, ISecurityEventWriter securityEvents, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<CreateOrgUnitCommand, CreatedResponse<OrgUnitId>>(recheck, execution)
{
    protected override async ValueTask<Result<CreatedResponse<OrgUnitId>>> ExecuteAsync(CreateOrgUnitCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var loaded = await trees.GetForUpdateAsync(OrgTreeId.Default, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var added = loaded.Value.AddUnit(command.ParentId, command.Name, command.Code);
        if (added.IsFailure)
        {
            return added.Error;
        }

        AuthorizationChangeEvents.Write(securityEvents, "OrgUnit", added.Value.Id.Value, "OrgUnitCreated");

        // 组织单元属于组织树，返回整棵树保存后的版本号
        return new CreatedResponse<OrgUnitId>(added.Value.Id, OwnerId: OrgTreeId.Default.Value);
    }
}
