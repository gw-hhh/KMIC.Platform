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
            .WithDescription("将来源模块发件箱中指定序号区间内、已投递的消息补投给指定订阅，已接收的消息不重复写入。单次最多 5000 条，超限请缩小区间。已终结的订阅须先重新激活。");

    private static async Task<IResult> HandleAsync(RedeliverRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RedeliverCommand(request.SourceModule, request.MessageType, request.Subscription, request.FromSeq, request.ToSeq), ct)).ToOk();
}
