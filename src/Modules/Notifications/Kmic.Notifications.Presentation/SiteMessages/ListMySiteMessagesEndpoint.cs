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
            .WithDescription("只有自己的消息。默认未读在前、新的在前，可以只看未读或已读、按类别筛选，分页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListMySiteMessagesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMySiteMessagesQuery(request.Unread, request.Category, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}
