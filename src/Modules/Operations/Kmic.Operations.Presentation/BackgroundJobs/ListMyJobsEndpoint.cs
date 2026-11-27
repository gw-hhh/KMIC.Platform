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
            .WithSummary("查询本人提交的后台作业")
            .WithDescription("查询本人在各模块提交的作业，按提交时间倒序游标分页（传 nextCursor 到 after）");

    private static async Task<IResult> HandleAsync([AsParameters] ListMyJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMyJobsQuery(request.Status, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
