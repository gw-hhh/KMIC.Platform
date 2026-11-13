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
            .WithDescription("交给运行着调度器的实例执行，几秒内开始；执行记录里触发方式是“手动”、触发人是操作人，结果到执行记录里看。任务正在运行时返回 409 Operations.JobAlreadyRunning，不排队。停用的任务也可以手动执行。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RunJobNowCommand(name), ct)).ToNoContent();
}
