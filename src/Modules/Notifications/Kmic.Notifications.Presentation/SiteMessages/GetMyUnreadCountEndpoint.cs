using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.SiteMessages.UnreadCount;

namespace Kmic.Notifications.Presentation.SiteMessages;

internal sealed class GetMyUnreadCountEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/site-messages/unread-count", HandleAsync)
            .Sends<GetMyUnreadCountQuery>()
            .WithSummary("查询我的未读站内消息数")
            .WithDescription("前端每 30 秒轮询一次，响应很小。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMyUnreadCountQuery(), ct)).ToOk();
}
