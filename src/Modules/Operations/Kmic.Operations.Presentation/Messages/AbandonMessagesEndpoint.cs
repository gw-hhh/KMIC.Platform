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
            .WithDescription("批量放弃收件箱死信（单批最多 200 条，原因必填）；未通过接收方放弃校验的项保持原状态，仅可重试");

    private static async Task<IResult> HandleAsync(AbandonMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonMessagesCommand(request.Messages, request.Reason), ct)).ToOk();
}
