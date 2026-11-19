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
            .WithSummary("放弃收件箱死信")
            .WithDescription("支持单条或批量操作，最多 200 条，原因必填。放弃后消息不再处理。接收方的放弃检查未通过时，保持原状态并在逐条结果中说明原因，只能重试。仅支持收件箱死信。");

    private static async Task<IResult> HandleAsync(AbandonMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonMessagesCommand(request.Messages, request.Reason), ct)).ToOk();
}
