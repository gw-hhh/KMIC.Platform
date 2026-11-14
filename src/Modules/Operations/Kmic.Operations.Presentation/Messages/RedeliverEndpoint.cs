using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Routes;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class RedeliverEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/redeliver", HandleAsync)
            .Sends<RedeliverCommand>()
            .WithSummary("补投")
            .WithDescription("把来源模块发件箱里已经投递过的、序号落在区间里的消息，再投递给指定订阅；订阅收到过的不会重复写入。一次最多 5000 条，超过要缩小区间。订阅已终结时要先重新激活。必须带幂等键。");

    private static async Task<IResult> HandleAsync(RedeliverRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RedeliverCommand(request.SourceModule, request.MessageType, request.Subscription, request.FromSeq, request.ToSeq), ct)).ToOk();
}
