using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.Mine;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class CancelMyJobEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/my-jobs/{module}/{id}/cancel", HandleAsync)
            .Sends<CancelMyJobCommand>()
            .WithSummary("请求取消本人提交的后台作业")
            .WithDescription("仅可取消本人作业（不存在或属他人返回 404）；排队中直接取消，运行中标记取消中，已结束或取消中返回 409");

    private static async Task<IResult> HandleAsync(
        [Description("作业所属模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        ICommandSender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new CancelMyJobCommand(module, id), ct)).ToNoContent();
}
