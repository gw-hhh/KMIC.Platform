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
            .WithDescription("仅可重新激活已终结订阅以恢复投递；终结期间的消息不会自动补投，须核对缺口后补投");

    private static async Task<IResult> HandleAsync(ReactivateRouteRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReactivateRouteCommand(request.MessageType, request.Subscription), ct)).ToNoContent();
}
