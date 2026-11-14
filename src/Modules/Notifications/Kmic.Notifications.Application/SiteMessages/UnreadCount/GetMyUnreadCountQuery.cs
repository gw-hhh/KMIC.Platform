using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.UnreadCount;

/// <summary>
/// 查询本人的未读站内消息数
/// </summary>
/// <remarks>
/// 供前端轮询未读数，不改变消息状态
/// </remarks>
[AllowAuthenticated]
public sealed record GetMyUnreadCountQuery : IQuery<UnreadCountResponse>;
