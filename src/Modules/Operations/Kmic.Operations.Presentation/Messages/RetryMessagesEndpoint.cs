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
            .WithDescription("批量重试死信（单批最多 200 条），清零重试次数并恢复待处理；逐条返回结果，不存在或状态不符不影响其他项");

    private static async Task<IResult> HandleAsync(RetryMessagesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryMessagesCommand(request.Messages), ct)).ToOk();
}
