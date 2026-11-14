using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Routes;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ReactivateRouteEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/messages/routes/reactivate", HandleAsync)
            .Sends<ReactivateRouteCommand>()
            .WithSummary("重新激活订阅")
            .WithDescription("只有已终结的订阅能重新激活，恢复成生效，从现在起重新投递。终结期间漏掉的消息不会自动补回，要核对缺口区间再补投。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync(ReactivateRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReactivateRouteCommand(request.MessageType, request.Subscription), ct)).ToNoContent();
}
