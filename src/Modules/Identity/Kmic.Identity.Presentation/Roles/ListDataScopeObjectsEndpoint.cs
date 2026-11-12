using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Catalogs;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class ListDataScopeObjectsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/data-scope-objects", HandleAsync)
            .Sends<ListDataScopeObjectsQuery>()
            .WithSummary("查询数据范围对象目录")
            .WithDescription("可以单独设置范围等级的对象。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDataScopeObjectsQuery(), ct)).ToOk();
}
