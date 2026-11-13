using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.UnreadCount;

/// <summary>
/// 查询我有几条未读的站内消息
/// </summary>
/// <remarks>
/// 前端每 30 秒轮询一次，只读、响应很小
/// </remarks>
[AllowAuthenticated]
public sealed record GetMyUnreadCountQuery : IQuery<UnreadCountResponse>;
