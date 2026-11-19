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
            .WithSummary("重试一条失败的投递")
            .WithDescription("仅可重试失败且未放弃的投递，否则返回 422；含密通知失败时内容已清除，一律不能重试。重试后重新排队，领取次数清零。成功返回 204。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeliveryCommand(id), ct)).ToNoContent();
}
