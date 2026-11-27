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
            .WithSummary("补投消息")
            .WithDescription("按发件箱序号区间将已投递消息补投给指定订阅（单次最多 5000 条，已接收不重复写入）；已终结订阅须先激活");

    private static async Task<IResult> HandleAsync(RedeliverRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RedeliverCommand(request.SourceModule, request.MessageType, request.Subscription, request.FromSeq, request.ToSeq), ct)).ToOk();
}
