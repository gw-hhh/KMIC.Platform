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
            .WithSummary("立即执行一次定时任务")
            .WithDescription("由运行调度器的实例执行，可在执行记录中查看结果，触发方式为手动并记录操作人。检查到任务正在运行时返回 409 Operations.JobAlreadyRunning，不提交触发请求；停用的任务也可手动执行。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RunJobNowCommand(name), ct)).ToNoContent();
}
