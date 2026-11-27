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
            .WithDescription("立即排队一次手动执行（不等待完成，结果见执行记录；已停用任务亦可触发）；正在执行、即将触发或已有手动排队时返回 409 Operations.JobAlreadyRunning");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RunJobNowCommand(name), ct)).ToNoContent();
}
