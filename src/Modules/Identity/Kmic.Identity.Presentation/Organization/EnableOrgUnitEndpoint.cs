using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Enable;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class EnableOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/org-units/{id}/enable", HandleAsync)
            .Sends<EnableOrgUnitCommand>()
            .WithSummary("启用组织单元")
            .WithDescription("它的上级必须是启用的。");

    private static async Task<IResult> HandleAsync([Description("组织单元 ID")] OrgUnitId id, OrgUnitVersionRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EnableOrgUnitCommand(request.ExpectedVersion, id), ct)).ToOk();
}
