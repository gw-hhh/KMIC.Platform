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
            .WithDescription("遍历每个模块自己的作业表，按提交时间倒序归并，游标分页：响应里的 nextCursor 作为下一页的 after，没有 nextCursor 就是最后一页。能看到状态、进度、结果摘要和错误，看不到作业参数。");

    private static async Task<IResult> HandleAsync([AsParameters] ListBackgroundJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBackgroundJobsQuery(request.Module, request.JobType, request.Status, request.SubmittedBy, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
