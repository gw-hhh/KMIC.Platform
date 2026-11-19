using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.RunNow;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class RunJobNowEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/scheduled-jobs/{name}/run", HandleAsync)
            .Sends<RunJobNowCommand>()
            .WithSummary("手动触发一次定时任务")
            .WithDescription("提交手动触发请求后返回，不等待任务完成；结果见执行记录，其中保留操作人。任务正在执行、即将按计划执行或已有手动触发在排队时返回 409 Operations.JobAlreadyRunning，不提交触发请求；停用的任务也可手动执行。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RunJobNowCommand(name), ct)).ToNoContent();
}
