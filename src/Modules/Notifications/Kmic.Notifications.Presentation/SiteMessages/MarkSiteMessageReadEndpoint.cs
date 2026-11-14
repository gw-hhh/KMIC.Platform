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
            .WithDescription("只能标记当前用户的消息。消息不存在或属于其他用户时返回 404；成功返回 204。消息已读时保留原已读时间。");

    private static async Task<IResult> HandleAsync([Description("消息 ID")] SiteMessageId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MarkSiteMessageReadCommand(id), ct)).ToNoContent();
}
