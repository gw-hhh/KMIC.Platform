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
            .WithSummary("恢复定时任务默认设置")
            .WithDescription("清空自定义启用状态和执行时间，保留设置记录供追溯，后续使用代码默认值。没有设置记录时返回 404。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("任务名")] string name, RestoreJobDefaultRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RestoreJobDefaultCommand(name, request.ExpectedVersion), ct)).ToOk();
}
