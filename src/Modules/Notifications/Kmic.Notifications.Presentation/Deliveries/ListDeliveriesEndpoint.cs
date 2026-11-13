using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Deliveries.List;

namespace Kmic.Notifications.Presentation.Deliveries;

internal sealed class ListDeliveriesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/deliveries", HandleAsync)
            .Sends<ListDeliveriesQuery>()
            .Sortable(DeliverySort.Map)
            .WithSummary("查询投递记录")
            .WithDescription("每条是把一条通知用一个渠道发给一个收件人：状态、脱敏地址、尝试次数、失败原因，不含正文。可以按收件用户、渠道、状态、模板、创建时间筛选，分页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListDeliveriesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ListDeliveriesQuery(request.RecipientUserId, request.Channel, request.Status, request.TemplateKey, request.From, request.To, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)),
            ct)).ToOk();
}
