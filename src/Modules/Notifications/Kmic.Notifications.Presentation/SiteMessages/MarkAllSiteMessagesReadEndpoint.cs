using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.SiteMessages.MarkRead;

namespace Kmic.Notifications.Presentation.SiteMessages;

internal sealed class MarkAllSiteMessagesReadEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/site-messages/read-all", HandleAsync)
            .Sends<MarkAllSiteMessagesReadCommand>()
            .WithSummary("将我的全部未读站内消息标为已读")
            .WithDescription("返回本次标记为已读的消息数。");

    private static async Task<IResult> HandleAsync(ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MarkAllSiteMessagesReadCommand(), ct)).ToOk();
}
