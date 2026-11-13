using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.SiteMessages.MarkRead;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Presentation.SiteMessages;

internal sealed class MarkSiteMessageReadEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/site-messages/{id}/read", HandleAsync)
            .Sends<MarkSiteMessageReadCommand>()
            .WithSummary("把一条站内消息标为已读")
            .WithDescription("已经读过的再标一次什么也不改。别人的消息和不存在的一样，返回 404。成功返回 204。");

    private static async Task<IResult> HandleAsync([Description("消息 ID")] SiteMessageId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MarkSiteMessageReadCommand(id), ct)).ToNoContent();
}
