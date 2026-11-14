using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.SiteMessages.List;

namespace Kmic.Notifications.Presentation.SiteMessages;

internal sealed class ListMySiteMessagesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/site-messages", HandleAsync)
            .Sends<ListMySiteMessagesQuery>()
            .Sortable(SiteMessageSort.Map)
            .WithSummary("查询我的站内消息")
            .WithDescription("分页查询本人的站内消息，默认未读在前、发送时间倒序。支持按已读状态和类别筛选。");

    private static async Task<IResult> HandleAsync([AsParameters] ListMySiteMessagesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMySiteMessagesQuery(request.Unread, request.Category, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
