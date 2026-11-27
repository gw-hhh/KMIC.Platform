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
            .WithDescription("汇总各模块作业状态、进度与错误信息（不含参数），按提交时间倒序游标分页（传 nextCursor 到 after）");

    private static async Task<IResult> HandleAsync([AsParameters] ListBackgroundJobsRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListBackgroundJobsQuery(request.Module, request.JobType, request.Status, request.SubmittedBy, new CursorRequest(request.After, request.Limit)), ct)).ToOk();
}
