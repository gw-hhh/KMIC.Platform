using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Catalogs;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class ListPermissionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/permissions", HandleAsync)
            .Sends<ListPermissionsQuery>()
            .WithSummary("查询权限点目录")
            .WithDescription("全部模块声明的权限点，按模块与分组排好。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListPermissionsQuery(), ct)).ToOk();
}
