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
            .WithDescription("新角色不含权限点，默认数据范围按请求设置。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(CreateRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateRoleCommand(request.Name, request.Description, request.SortOrder, request.DefaultScopeLevel), ct))
            .ToCreated(id => $"/api/v1/identity/roles/{id.Value}");
}
