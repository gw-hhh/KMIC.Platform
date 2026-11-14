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
            .WithSummary("查询投递详情")
            .WithDescription("返回渲染后的主题和正文。含密投递发送成功、最终失败或跳过后清空正文。查看会记录读取审计。");

    private static async Task<IResult> HandleAsync([Description("投递 ID")] DeliveryId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetDeliveryQuery(id), ct)).ToOk();
}
