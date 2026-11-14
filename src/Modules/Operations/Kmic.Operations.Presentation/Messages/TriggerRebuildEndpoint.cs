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
            .WithDescription("给订阅手动排一轮重建，订阅要声明了重建方式。已经有排队或进行中、暂停等确认的一轮时返回 409。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync(RebuildRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new TriggerRebuildCommand(request.Subscription), ct)).ToNoContent();
}
