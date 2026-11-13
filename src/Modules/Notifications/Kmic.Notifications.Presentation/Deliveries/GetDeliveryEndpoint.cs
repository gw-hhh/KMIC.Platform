using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Deliveries.Get;
using Kmic.Notifications.Domain.Deliveries;

namespace Kmic.Notifications.Presentation.Deliveries;

internal sealed class GetDeliveryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/deliveries/{id}", HandleAsync)
            .Sends<GetDeliveryQuery>()
            .WithSummary("查询一条投递")
            .WithDescription("包含渲染后的主题和正文。含密模板的投递发完或最终失败后，正文已经清空。查看会记读审计。");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetDeliveryQuery(id), ct)).ToOk();
}
