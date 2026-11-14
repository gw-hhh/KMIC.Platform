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
            .WithDescription("放弃暂停的一轮，并清除订阅的需要初始化标记：此后不会自动重建，要重建由运维手动触发。没有暂停的一轮返回 409。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonRebuildCommand(request.Subscription), ct)).ToNoContent();
}
