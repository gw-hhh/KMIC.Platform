using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Rebuilds;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class TriggerRebuildEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/rebuilds/trigger", HandleAsync)
            .Sends<TriggerRebuildCommand>()
            .WithSummary("手动触发读模型重建")
            .WithDescription("为已声明重建方式的订阅提交重建作业。已有未结束的重建任务时返回 409。");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new TriggerRebuildCommand(request.Subscription), ct)).ToNoContent();
}
