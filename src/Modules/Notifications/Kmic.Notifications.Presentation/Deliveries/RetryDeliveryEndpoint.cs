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
            .WithDescription("放回队列，尝试次数清零。只有失败、而且没有放弃的投递可以重试，否则返回 422；含密通知超过有效期的不能重试。成功返回 204。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeliveryCommand(id), ct)).ToNoContent();
}
