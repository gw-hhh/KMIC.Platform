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
            .WithDescription("为已声明重建方式的订阅提交重建作业。已有排队、执行中或等待确认的重建时返回 409。成功返回 204。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new TriggerRebuildCommand(request.Subscription), ct)).ToNoContent();
}
