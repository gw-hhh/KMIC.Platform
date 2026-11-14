using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 把我的全部未读站内消息标为已读
/// </summary>
[AllowAuthenticated]
[NotAudited("个人已读状态无需业务审计")]
public sealed record MarkAllSiteMessagesReadCommand : ICommand<MarkedReadResponse>;
