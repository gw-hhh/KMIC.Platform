using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.List;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class ListSpecimensEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/specimens", HandleAsync)
            .Sends<ListSpecimensQuery>()
            .Sortable(SpecimenSort.Map)
            .WithSummary("查询示例标本列表")
            .WithDescription("按当前用户的数据范围返回标本，分页。页码超过末页返回空列表，每页数量超出 1 到 200 返回 400。");

    private static async Task<IResult> HandleAsync([AsParameters] ListSpecimensRequest request, IQuerySender sender, CancellationToken ct)
    {
        var query = new ListSpecimensQuery(request.Q, request.Status, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort));
        return (await sender.SendAsync(query, ct)).ToOk();
    }
}
