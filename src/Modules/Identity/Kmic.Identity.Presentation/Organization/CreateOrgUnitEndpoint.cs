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
            .WithDescription("在已启用的组织单元下新增下级，排在同级末尾。响应中的 version 是新增后的组织树版本号。");

    private static async Task<IResult> HandleAsync(CreateOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateOrgUnitCommand(request.ExpectedVersion, request.ParentId, request.Name, request.Code), ct))
            .ToCreated(_ => "/api/v1/identity/org-tree");
}
