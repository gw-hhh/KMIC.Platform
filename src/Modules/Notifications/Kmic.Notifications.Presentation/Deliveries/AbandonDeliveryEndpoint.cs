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
            .WithSummary("放弃失败的投递")
            .WithDescription("仅可放弃失败且未放弃的投递（否则返回 422）；放弃后不可重试，可按保留期清理");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, AbandonDeliveryRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AbandonDeliveryCommand(id, request.Reason), ct)).ToNoContent();
}
