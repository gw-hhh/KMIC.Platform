using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.UnreadCount;

/// <summary>
/// 查询本人的未读站内消息数
/// </summary>
[AllowAuthenticated]
public sealed record GetMyUnreadCountQuery : IQuery<UnreadCountResponse>;
