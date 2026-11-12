using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Create;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class CreateRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/roles", HandleAsync)
            .Sends<CreateRoleCommand>()
            .WithSummary("新建角色")
            .WithDescription("新建一个没有任何权限点的角色，之后用设置权限点与数据范围的接口配置。必须带幂等键。");

    private static async Task<IResult> HandleAsync(CreateRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateRoleCommand(request.Name, request.Description, request.SortOrder, request.DefaultScopeLevel), ct))
            .ToCreated(id => $"/api/v1/identity/roles/{id.Value}");
}
