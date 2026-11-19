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
            .WithDescription("放弃等待确认的重建并清除当前初始化标记。如需再次重建，请手动触发。没有等待确认的重建时返回 409；");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonRebuildCommand(request.Subscription), ct)).ToNoContent();
}
