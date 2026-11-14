using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class ListStorageIssuesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/storage-issues", HandleAsync)
            .Sends<ListStorageIssuesQuery>()
            .Sortable(StorageIssueSort.Map)
            .WithSummary("查询存储问题")
            .WithDescription("对账发现库里有记录而存储里找不到对象的文件，分页。“可以还原”的对象版本还在，运维按手册还原后用“标为已解决”关闭。");

    private static async Task<IResult> HandleAsync([AsParameters] ListStorageIssuesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ListStorageIssuesQuery(request.Status, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)),
            ct)).ToOk();
}
