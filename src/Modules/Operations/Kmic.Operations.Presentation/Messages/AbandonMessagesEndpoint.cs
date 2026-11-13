using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Abandon;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class AbandonMessagesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/abandon", HandleAsync)
            .Sends<AbandonMessagesCommand>()
            .WithSummary("放弃收件箱里的死信，单条或批量")
            .WithDescription("放弃意味着这些消息永远不会被处理，原因必填。放弃之前先调接收方为这种消息注册的放弃检查，不让放弃的不改状态，回执里写明原因，只能重试。只有收件箱的死信能放弃。一次最多 200 条。必须带幂等键。");

    private static async Task<IResult> HandleAsync(AbandonMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonMessagesCommand(request.Messages, request.Reason), ct)).ToOk();
}
