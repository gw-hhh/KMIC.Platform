using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Reorder;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class ReorderOrgUnitsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/org-units/{parentId}/children-order", HandleAsync)
            .Sends<ReorderOrgUnitsCommand>()
            .WithSummary("调整某个单元下全部直接下级的次序")
            .WithDescription("整体提交：必须包含该单元下的全部直接下级，且不重复。");

    private static async Task<IResult> HandleAsync([Description("上级组织单元 ID")] OrgUnitId parentId, ReorderOrgUnitsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReorderOrgUnitsCommand(request.ExpectedVersion, parentId, request.OrderedChildren), ct)).ToOk();
}
