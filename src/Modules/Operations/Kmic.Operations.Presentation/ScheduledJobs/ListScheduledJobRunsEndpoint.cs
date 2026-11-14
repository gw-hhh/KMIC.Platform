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
            .WithSummary("查询定时任务执行记录")
            .WithDescription("按开始时间倒序，使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。任务声明已移除时，仍可查询历史记录。");

    private static async Task<IResult> HandleAsync(
        [Description("任务名，如 Identity.PurgeSessions")] string name,
        [AsParameters] ListScheduledJobRunsRequest request,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new ListScheduledJobRunsQuery(name, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
