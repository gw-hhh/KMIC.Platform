using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Retry;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class RetryMessagesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/retry", HandleAsync)
            .Sends<RetryMessagesCommand>()
            .WithSummary("重试死信，单条或批量")
            .WithDescription("状态回到待处理，次数清零，马上可以被领取；一次最多 200 条。每条各有各的回执，已经不是死信的、不存在的不影响别的。重试不保证成功：问题没解决的消息会再次失败。必须带幂等键。");

    private static async Task<IResult> HandleAsync(RetryMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryMessagesCommand(request.Messages), ct)).ToOk();
}
