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
            .WithSummary("重试死信")
            .WithDescription("支持单条或批量操作，最多 200 条。将死信恢复为待处理，重试次数清零并允许立即领取。逐条返回结果，不存在或状态不符的消息不影响其他消息。未解决的问题仍可能导致重试失败。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(RetryMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryMessagesCommand(request.Messages), ct)).ToOk();
}
