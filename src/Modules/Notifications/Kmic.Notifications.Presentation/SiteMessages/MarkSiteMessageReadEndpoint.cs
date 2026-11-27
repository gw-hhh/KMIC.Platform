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
            .WithSummary("将我的指定站内消息标为已读")
            .WithDescription("仅标记本人消息（不存在或属他人返回 404）；已读消息保留原已读时间");

    private static async Task<IResult> HandleAsync([Description("消息 ID")] SiteMessageId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MarkSiteMessageReadCommand(id), ct)).ToNoContent();
}
