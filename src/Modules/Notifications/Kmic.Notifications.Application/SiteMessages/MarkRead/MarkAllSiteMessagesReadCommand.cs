using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 把我的全部未读站内消息标为已读
/// </summary>
[AllowAuthenticated]
[NotAudited("个人的已读状态，不是业务数据")]
public sealed record MarkAllSiteMessagesReadCommand : ICommand<MarkedReadResponse>;
