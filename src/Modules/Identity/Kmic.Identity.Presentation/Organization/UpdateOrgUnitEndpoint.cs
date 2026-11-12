using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Update;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class UpdateOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/org-units/{id}", HandleAsync)
            .Sends<UpdateOrgUnitCommand>()
            .WithSummary("修改组织单元的名称与编码");

    private static async Task<IResult> HandleAsync([Description("组织单元 ID")] OrgUnitId id, UpdateOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UpdateOrgUnitCommand(request.ExpectedVersion, id, request.Name, request.Code), ct)).ToOk();
}
