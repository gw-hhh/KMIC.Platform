using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.Restore;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class RestoreJobDefaultEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/scheduled-jobs/{name}/restore-default", HandleAsync)
            .Sends<RestoreJobDefaultCommand>()
            .WithSummary("把一个定时任务恢复成代码里的默认开关与执行时间")
            .WithDescription("设置行保留、两项清空，便于追溯，之后跟着代码里的默认值变化。一直没改过的任务没有可恢复的，返回 404。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, RestoreJobDefaultRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RestoreJobDefaultCommand(name, request.ExpectedVersion), ct)).ToOk();
}
