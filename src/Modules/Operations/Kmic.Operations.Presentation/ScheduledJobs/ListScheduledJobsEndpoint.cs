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
            .WithDescription("返回代码声明任务及其配置、调度状态与最近执行记录；已移除声明但仍存自定义设置的任务以 isDeclared 为 false 列出");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListScheduledJobsQuery(), ct)).ToOk();
}
