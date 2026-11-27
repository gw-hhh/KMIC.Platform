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
            .WithDescription("分页查询对象缺失的存储问题；“可还原”表示仍可读取记录中的对象版本，运维还原后可登记结果");

    private static async Task<IResult> HandleAsync([AsParameters] ListStorageIssuesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ListStorageIssuesQuery(request.Status, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)),
            ct)).ToOk();
}
