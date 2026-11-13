using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.Mine;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class ListMyJobsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/my-jobs", HandleAsync)
            .Sends<ListMyJobsQuery>()
            .WithSummary("查询我在各模块提交的后台作业")
            .WithDescription("登录就能看，只看得到自己提交的；外壳的“作业入口”用它。按提交时间倒序，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListMyJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMyJobsQuery(request.Status, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
