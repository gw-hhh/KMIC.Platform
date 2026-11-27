using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Routes;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class FinalizeRouteEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/routes/finalize", HandleAsync)
            .Sends<FinalizeRouteCommand>()
            .WithSummary("终结订阅")
            .WithDescription("在维护窗口终结“待终结”订阅（停投且旧构建不可自动恢复）；仍有活跃声明实例或未完成消息占用未到期时拒绝，可选择一并放弃未完成消息");

    private static async Task<IResult> HandleAsync(FinalizeRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new FinalizeRouteCommand(request.MessageType, request.Subscription, request.Reason, request.AbandonUnfinished), ct)).ToOk();
}
