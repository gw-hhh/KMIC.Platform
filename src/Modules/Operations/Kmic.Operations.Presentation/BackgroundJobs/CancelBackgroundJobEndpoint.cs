using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.Cancel;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class CancelBackgroundJobEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/background-jobs/{module}/{id}/cancel", HandleAsync)
            .Sends<CancelBackgroundJobCommand>()
            .WithSummary("请求取消一个后台作业")
            .WithDescription("排队中的直接取消，运行中的改为“取消中”，执行器在下次续期时停下。取消不是回滚，已提交的批次保留。作业已经结束返回 409 Operations.BackgroundJobFinished。必须带幂等键。");

    private static async Task<IResult> HandleAsync(
        [Description("作业所在的模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        ICommandSender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new CancelBackgroundJobCommand(module, id), ct)).ToNoContent();
}
