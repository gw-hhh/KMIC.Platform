using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 将当前用户的站内消息标为已读
/// </summary>
[AllowAuthenticated]
[NotAudited("个人已读状态无需业务审计")]
public sealed record MarkSiteMessageReadCommand(SiteMessageId Id) : ICommand;
