using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Rebuilds;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class AbandonRebuildEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/rebuilds/abandon", HandleAsync)
            .Sends<AbandonRebuildCommand>()
            .WithSummary("放弃读模型重建")
            .WithDescription("放弃待确认的重建并清除初始化标记，后续需手动重触发；无待确认任务返回 409");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonRebuildCommand(request.Subscription), ct)).ToNoContent();
}
