using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.List;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class ListBackgroundJobsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/background-jobs", HandleAsync)
            .Sends<ListBackgroundJobsQuery>()
            .WithSummary("查询各模块的后台作业")
            .WithDescription("汇总各模块的作业，按提交时间倒序返回状态、进度、结果摘要和错误，不返回作业参数。使用游标分页：将 nextCursor 作为下一页的 after，未返回 nextCursor 表示已到末页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListBackgroundJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBackgroundJobsQuery(request.Module, request.JobType, request.Status, request.SubmittedBy, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
