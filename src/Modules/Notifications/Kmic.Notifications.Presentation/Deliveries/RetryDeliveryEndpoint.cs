using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Deliveries.Retry;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Presentation.Deliveries;

internal sealed class RetryDeliveryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/deliveries/{id}/retry", HandleAsync)
            .Sends<RetryDeliveryCommand>()
            .WithSummary("重试失败的投递")
            .WithDescription("仅可重试非敏感且未放弃的失败投递（否则返回 422）；重试后领取次数清零并重新入队");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeliveryCommand(id), ct)).ToNoContent();
}
