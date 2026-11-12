using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Move;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class MoveOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/org-units/{id}/move", HandleAsync)
            .Sends<MoveOrgUnitCommand>()
            .WithSummary("移动组织单元")
            .WithDescription("连同它的下级一起移到另一个单元之下，排在新上级的最后。不能移到自己或自己的下级之下，根单元不能移动。");

    private static async Task<IResult> HandleAsync([Description("组织单元 ID")] OrgUnitId id, MoveOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MoveOrgUnitCommand(request.ExpectedVersion, id, request.NewParentId), ct)).ToOk();
}
