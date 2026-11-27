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
            .WithSummary("查询示例列表")
            .WithDescription("分页返回当前用户有权查看的示例；每页条数须为 1～200（否则返回 400），越界页码返回空列表");

    private static async Task<IResult> HandleAsync([AsParameters] ListSpecimensRequest request, IQuerySender sender, CancellationToken ct)
    {
        var query = new ListSpecimensQuery(request.Q, request.Status, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort));
        return (await sender.SendAsync(query, ct)).ToOk();
    }
}
