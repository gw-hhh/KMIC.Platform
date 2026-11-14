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
            .WithDescription("登录后可查询本人在各模块提交的作业，供页面中的作业入口使用。按提交时间倒序，使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListMyJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMyJobsQuery(request.Status, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
