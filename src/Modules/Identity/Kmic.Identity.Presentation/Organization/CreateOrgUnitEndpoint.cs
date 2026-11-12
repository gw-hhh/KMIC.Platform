using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Create;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class CreateOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/org-units", HandleAsync)
            .Sends<CreateOrgUnitCommand>()
            .WithSummary("添加组织单元")
            .WithDescription("在某个启用的单元下添加下级，排在同级最后。响应里的 version 是添加之后的组织树版本号。必须带幂等键。");

    private static async Task<IResult> HandleAsync(CreateOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateOrgUnitCommand(request.ExpectedVersion, request.ParentId, request.Name, request.Code), ct))
            .ToCreated(_ => "/api/v1/identity/org-tree");
}
