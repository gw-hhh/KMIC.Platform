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
            .WithDescription("变更上级后排在新同级末尾；上级未变时不调整顺序。不能移到自身或自身下级中，根单元不能移动。");

    private static async Task<IResult> HandleAsync([Description("组织单元 ID")] OrgUnitId id, MoveOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MoveOrgUnitCommand(request.ExpectedVersion, id, request.NewParentId), ct)).ToOk();
}
