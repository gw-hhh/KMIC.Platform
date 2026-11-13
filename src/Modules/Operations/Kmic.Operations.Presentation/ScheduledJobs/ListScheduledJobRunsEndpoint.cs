using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.Runs;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class ListScheduledJobRunsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/scheduled-jobs/{name}/runs", HandleAsync)
            .Sends<ListScheduledJobRunsQuery>()
            .WithSummary("查询一个定时任务的执行记录")
            .WithDescription("按开始时间倒序，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。已经从代码里删除的任务，留下的记录也能查。");

    private static async Task<IResult> HandleAsync(
        [Description("任务名，如 Identity.PurgeSessions")] string name,
        [AsParameters] ListScheduledJobRunsRequest request,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new ListScheduledJobRunsQuery(name, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
