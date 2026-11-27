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
            .WithSummary("请求取消后台作业")
            .WithDescription("排队中直接取消，运行中标记取消中（已提交批次不回滚）；已结束或取消中返回 409 Operations.BackgroundJobFinished");

    private static async Task<IResult> HandleAsync(
        [Description("作业所属模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        ICommandSender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new CancelBackgroundJobCommand(module, id), ct)).ToNoContent();
}
