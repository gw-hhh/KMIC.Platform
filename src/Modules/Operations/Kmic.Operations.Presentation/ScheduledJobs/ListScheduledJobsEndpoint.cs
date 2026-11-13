using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.List;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class ListScheduledJobsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/scheduled-jobs", HandleAsync)
            .Sends<ListScheduledJobsQuery>()
            .WithSummary("查询全部定时任务")
            .WithDescription("模块的和框架自带的都列出来，每项带代码里的默认值、运维改过的覆盖值、调度器里实际生效的状态（下次触发时间、是否已同步）、最近一次执行和当前用户可用的动作。代码里已经删除而覆盖设置还留着的任务也在，标为已删除。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListScheduledJobsQuery(), ct)).ToOk();
}
