using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Deliveries.Abandon;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Presentation.Deliveries;

internal sealed class AbandonDeliveryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/deliveries/{id}/abandon", HandleAsync)
            .Sends<AbandonDeliveryCommand>()
            .WithSummary("放弃一条失败的投递")
            .WithDescription("状态仍是失败，加上放弃标记，不再出现在待处理清单里。只有失败、而且还没放弃的投递可以放弃，否则返回 422。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, AbandonDeliveryRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonDeliveryCommand(id, request.Reason), ct)).ToNoContent();
}
